using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace JBGE {
	public class VMBase {
		// Commands (8 bit)
		private const byte NOP = 0x0; // No operation
		private const byte VMC = 0x1; // Virtual Machine Call
		private const byte VAR = 0x2; // Declare/Operate on a variable
		private const byte MOV = 0x3; // Assign value to pointer variable or variable
		private const byte JMP = 0x4; // Jump to label
		private const byte JMS = 0x5; // Jump to subroutine
		private const byte RET = 0x6; // Return from subroutine
		private const byte ADD = 0x7; // Add
		private const byte SUB = 0x8; // Subtract
		private const byte MLT = 0x9; // Multiply
		private const byte DIV = 0xA; // Divide
		private const byte MOD = 0xB; // Modulas
		private const byte SHL = 0xC; // Shift left
		private const byte SHR = 0xD; // Shift right
		private const byte CAN = 0xE; // Conditional Logical AND (&&)
		private const byte COR = 0xF; // Conditional Logical OR (||)
		private const byte SKE = 0x10; // Skip if equal
		private const byte AND = 0x11; // Logical AND
		private const byte LOR = 0x12; // Logical OR
		private const byte XOR = 0x13; // Exclusive OR
		private const byte PSH = 0x14; // Push to stack
		private const byte POP = 0x15; // Pop from stack
		private const byte LEQ = 0x16; // Logical Operator ==
		private const byte LNE = 0x17; // Logical Operator !=
		private const byte LSE = 0x18; // Logical Operator <=
		private const byte LGE = 0x19; // Logical Operator >=
		private const byte LGS = 0x1A; // Logical Operator <
		private const byte LGG = 0x1B; // Logical Operator >
		private const byte UMN = 0x1C; // UnaryMinus -
		private const byte UPL = 0x1D; // UnaryPlus +
		private const byte UNT = 0x1E; // UnaryNot !
		private const byte BUC = 0x1F; // Bitwise Unary Complement ~

		// Type (8 bit)
		private const byte TYPE_NULL = 0x0; // null
		private const byte TYPE_PTR = 0x1; // Operation manipulates on a pointer
		private const byte TYPE_IMM = 0x2; // Operation manipulates on an immediate value
		private const byte TYPE_UVAR8 = 0x3; // Operation manipulates on 8 bit unsigned variable
		private const byte TYPE_SVAR8 = 0x4; // Operation manipulates on 8 bit signed variable
		private const byte TYPE_UVAR16 = 0x5; // Operation manipulates on 16 bit unsigned variable
		private const byte TYPE_SVAR16 = 0x6; // Operation manipulates on 16 bit signed variable		
		private const byte TYPE_UVAR32 = 0x7; // Operation manipulates on 32 bit unsigned variable
		private const byte TYPE_SVAR32 = 0x8; // Operation manipulates on 32 bit signed variable
		private const byte TYPE_UVAR64 = 0x9; // Operation manipulates on 64 bit unsigned variable
		private const byte TYPE_SVAR64 = 0xA; // Operation manipulates on 64 bit signed variable

		// Pre-defined VMC call IDs (0 to 255 are preserved)
		private const int VMC_PRINT = 0;
		private const int VMC_MALLOC = 1;
		private const int VMC_REG_MALLOC_GROUP_SIZE = 2;
		private const int VMC_RUN_GC = 3;
		private const int VMC_SET_FLOAT_MULTBY_FACTOR = 4;
		private const int VMC_FREE = 5;
		private const int VMC_SIZEOF_ALLOCMEM = 6;
		private const int VMC_FILE_OPEN = 7;
		private const int VMC_FILE_CLEAR_BUFFER = 8;
		private const int VMC_FILE_APPEND_VM_BYTES = 9;
		private const int VMC_FILE_SAVE = 10;
		private const int VMC_FILE_READ_VM_BYTES = 11;
		private const int VMC_FILE_LOAD = 12;

		// OpCode size (Command(1 byte) + OpType(1 byte) = 2 bytes)
		private const int MAX_OPCODE_BYTE_SIZE = 2;
		// Value always have a signed long size (the size of long may vary by underlying architecture)
		private int MaxOpcodeValueSize;
		// The decimal point precision multiplying factor that we've applied to float number during compile phase
		// We will need to divide this number with the factor that we multiplied with, in order to revert back to a float number
		// This multiply factor will be embedded inside our binary code with a VMC call to assign it to our Virtual Machine variable (FloatMultByFactor)
		protected double DecPtMltFactor = 0;

		// Stores all the binary code
		protected List<byte> BinCode = new List<byte>();
		// Maximum bit size allowed
		private int MaxBitSize;
		// Program counter
		private int pc;
		// A variable pointer that indicates which variable we are going to operate on
		private long UseVar;
		// Stores the opType of the currently selected variable
		private byte UseVarOpType;
		// Stores the total number of static variables (variables defined already in code)
		//public int numDeclaredVars;
		// Stores the next variable ID to be used
		//public long varIDCounter;

		// Virtual stack used for storing subroutine caller addresses for JMS commands (FILO)
		private List<int> SubReturnIndexStack = new List<int>();
		// Virtual stack used by PSH and POP commands for common calculation purposes (FILO)
		private List<long> CommonValueStack = new List<long>();
		// Virtual memory
		public List<byte> VirtualMemory = new List<byte>();

		// Table of variable ID and its corresponding virtual memory address (long => variable ID, int => variable address)
		public List<int> VarAddrTable = new List<int>();
		// Table of variable ID with indication of how many memory in bytes it is consuming (long => variable ID, int => variable size)
		public List<int> VarMemSizeTable = new List<int>();
		// Table of addresses/total bytes of allocated memory in the virtual address (long => address in virtual memory, int => amount of bytes allocated)
		public Dictionary<long, int> MallocAddressSizeTable = new Dictionary<long, int>();
		// Table of static instance objects registered as variables (long => points to the variable inside VarAddrTable)
		public List<int> StaticInstanceObjectList = new List<int>();

		// Temporary data storage used to convert byte data between short/int/long data
		private byte[] tmpBytes = new byte[8];

		private string fileStreamFileName = "";
		private List<byte> fileStreamBuffer = new List<byte>();

		//public string debugDumpCode = "";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="binFile">.bin file to be executed</param>
		/// <param name="bitMode">
		/// Size of maximum bit size allowed which should have the same size as specified on compilation
		/// (i.e. if -b32 was specified in the compile options, bitMode should be 32)
		/// </param>
		public VMBase(int maxBitSize = 32) {
			MaxBitSize = maxBitSize;
			MaxOpcodeValueSize = MaxBitSize / 8;
		}

		/// <summary>
		/// Load .bin file to be executed
		/// </summary>
		/// <param name="binFile">.bin file to be executed</param>
		public virtual void LoadBinFile(string binFile) {
			// Read all byte codes
			byte[] byteCodes = File.ReadAllBytes(binFile);
			BinCode.Clear();
			for(int i = 0; i < byteCodes.Length; i++) BinCode.Add(byteCodes[i]);
			// Initialize VM
			InitializeVM();
		}

		/// <summary>
		/// Compiles script code to binary
		/// </summary>
		/// <param name="scriptFile">.cs Script file to compile</param>
		/// <param name="isWriteBinFile">Set to true if writing to local file</param>
		/// <returns>The compiled byte code</returns>
		public byte[] CompileScript(string scriptFile, bool isWriteBinFile = true) {
			CompileOptions compileOptions = new CompileOptions();
			compileOptions.FileName = scriptFile;
			compileOptions.BinFileName = Path.ChangeExtension(scriptFile, ".bytes").Replace("Assets/DScripts~", "Assets/Bin");
			compileOptions.IsOutputIntermCode = false;
			compileOptions.MaxBitSize = 32;
			compileOptions.DecimalPointPrecision = 8;
			JBScript.Compiler c = new JBScript.Compiler();
			return c.Compile(compileOptions, isWriteBinFile);
		}

		/// <summary>Initialize VM</summary>
		protected virtual void InitializeVM() {
			pc = 0;
			UseVar = 0;
			UseVarOpType = 0;
			DecPtMltFactor = 0;
			//numDeclaredVars = 0;
			//varIDCounter = 0;
			// Initialize all virtual stacks and memory
			SubReturnIndexStack.Clear();
			CommonValueStack.Clear();
			VirtualMemory.Clear();
			VarAddrTable.Clear();
			VarMemSizeTable.Clear();
			MallocAddressSizeTable.Clear();
			StaticInstanceObjectList.Clear();
			fileStreamBuffer.Clear();
			// Register declared variables
			RegisterVariables();
		}

		/// <summary>
		/// Run the code once and register all the declared variables used in our code.
		/// The compiled .bin file is already organized with all declared variables placed on the top of file
		/// </summary>
		private void RegisterVariables() {
			// The first few sets of non VAR commands are JMS commands to the main entry point program, so we ignore it until we find the first VAR command
			bool isVarCmdFound = false;
			while(true) {
				// The first byte is the command
				byte cmd = BinCode[pc];
				// The second byte is the type
				byte type = BinCode[pc + 1];
				// Until we find the first VAR command, just ignore operation
				if(cmd == VAR && !isVarCmdFound) {
					isVarCmdFound = true;
				}
				// If we reached the first non VAR command, halt execution
				if(cmd != VAR && isVarCmdFound) {
					// Halt program execution
					break;
				}
				if(isVarCmdFound) {
					// The third byte onwards are the value (could be either a variable, pointer var, immediate)
					//int valByteSize = MaxBitSize / 8;
					//long value = GetBinCodeValue(pc + MAX_OPCODE_BYTE_SIZE, valByteSize, BinCode);
					// Deploy the variable to our virtual memory
					int byteSize = GetByteSizeFromOperationType(type);
					RegisterVarToVirtualMemory(pc + MAX_OPCODE_BYTE_SIZE, byteSize);
					// Store to our static instantiated varable list if type is a pointer variable
					if(type == TYPE_PTR) StaticInstanceObjectList.Add(VarAddrTable.Count - 1);
				}
				pc += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize;
				//numDeclaredVars++;
			}
			// Store the next variable ID counter
			//varIDCounter = numDeclaredVars;
			// Run reamining program after the VAR declarations (this will eventually run the init method defined in [DScript_Init] as well)
			Run(-1, pc);

			// Reset program counter to beginning of code
			pc = 0;
		}

		/// <summary>
		/// Get the byte size according to the specified architecture / MaxBitSize (maximum bit size allowed)
		/// </summary>
		/// <param name="opType">One of the operation types defined in the constants (TYPE_XXX)</param>
		/// <returns></returns>
		private byte GetByteSizeFromOperationType(byte opType) {
			byte byteSize = 0;
			switch(opType) {
				case TYPE_UVAR8: case TYPE_SVAR8: byteSize = 1; break;
				case TYPE_UVAR16: case TYPE_SVAR16: byteSize = 2; break;
				case TYPE_UVAR32: case TYPE_SVAR32: byteSize = 4; break;
				case TYPE_UVAR64: case TYPE_SVAR64: byteSize = 8; break;
				case TYPE_NULL: byteSize = 0; break;
				default: byteSize = (byte)(MaxBitSize / 8); break;
			}
			return byteSize;
		}

		/// <summary>
		/// Get the actual value (in long size) in the specified byte list
		/// </summary>
		/// <param name="startAddressIndex"></param>
		/// <param name="byteSize">Number of bytes to seek in the memory</param>
		/// <param name="byteCodeList">Reference to the byte list</param>
		/// <returns></returns>
		private long GetBinCodeValue(int startAddressIndex, int byteSize, List<byte> byteCodeList) {
			long value = 0;
			int addr = startAddressIndex;
			for(int i = 0; i < byteSize; i++) {
				value <<= 8;
				value |= byteCodeList[addr + i];
			}
			return value;
		}

		/// <summary>
		/// Add variable to virtual memory
		/// </summary>
		/// <param name="startBinCodeIndex">The start index of the BinCode to reference from</param>
		/// <param name="byteSize">Number of bytes to be stored to virtual memory</param>
		/// <param name="varID">The variable ID</param>
		private void RegisterVarToVirtualMemory(int startBinCodeIndex, int byteSize) {
			// Store the variable ID and its address
			VarAddrTable.Add(VirtualMemory.Count);
			// Store the variable ID and its size (in bytes)
			VarMemSizeTable.Add(byteSize);
			// Initialize the value of variable with 0
			for(int i = startBinCodeIndex; i < startBinCodeIndex + byteSize; i++) VirtualMemory.Add(0);
		}


		/// <summary>
		/// Given the virtual memory address and the value (long) to store, this method assigns a value to the virtual memory space
		/// </summary>
		/// <param name="vMemAddress">Beginning address of the virtual memory to store the value</param>
		/// <param name="byteSize">The byte size to load in</param>
		/// <param name="value">A long size value - which is reduced to the specified size depending on the operation type</param>
		private void SetValueToVirtualMemory(int vMemAddress, byte byteSize, long value) {
			byte[] valBytes = BitConverter.GetBytes(value);
			for(int i = 0; i < byteSize; i++) {
				VirtualMemory[vMemAddress + i] = valBytes[byteSize - i - 1];
			}
		}

		/// <summary>
		/// Get the value from the virtual memory from the given address
		/// </summary>
		/// <param name="vMemAddress">Address to seek into the virtual memory</param>
		/// <param name="byteSize">The opeartion type of the address we are refering to</param>
		/// <returns></returns>
		protected long GetValueFromVirtualMemory(int vMemAddress, byte byteSize) {
			long value = 0;
			// Initialize temporary byte storage to 0
			for(int i = 0; i < tmpBytes.Length; i++) tmpBytes[i] = 0;
			for(int i = 0; i < byteSize; i++) {
				tmpBytes[byteSize - 1 - i] = VirtualMemory[vMemAddress + i];
			}
			switch(byteSize) {
				case 1: value = (long)tmpBytes[0]; break;
				case 2: value = (long)BitConverter.ToInt16(tmpBytes, 0); break;
				case 4: value = (long)BitConverter.ToInt32(tmpBytes, 0); break;
				case 8: value = (long)BitConverter.ToInt64(tmpBytes, 0); break;
			}
			return value;
		}

		/// <summary>
		/// Sets the value to the virtual memory space depending on the target variable type
		/// </summary>
		/// <param name="binCodeValue">The value specified in our byte code</param>
		/// <param name="opType">The opeartion type of the binCodeValue</param>
		/// <param name="value">The value to be deployed to the virtual memory space</param>
		private void SetActualValueByType(long binCodeValue, byte opType, long value) {
			byte byteSize = GetByteSizeFromOperationType(opType);
			if(opType == TYPE_PTR) {
				// Get the value of the variable first as this value is the address pointer of the actual virtual memory space we need to refer to
				int variableAddress = VarAddrTable[(int)binCodeValue];
				int actualAddress = (int)GetValueFromVirtualMemory(variableAddress, byteSize);
				// What's the destination byte size? (the byte size of the actual variable that is pointed by this pointer var)
				// Which variable actually holds this "actualAddress"
				long actualVar = 0;
				/*
				foreach(KeyValuePair<long, int> item in VarAddrTable) {
					if(item.Value == actualAddress) {
						actualVar = item.Key;
						break;
					}
				}*/
				// Normally, we reference variables created after all static variables, so it may be quicker to search the variable list from the bottom of our list
				//int lastAddress = (int)VarAddrTable.Keys.Last();
				int lastAddress = VarAddrTable.Count() - 1;
				for(int i = lastAddress; i >= 0; i--) {
					//if (!VarAddrTable.ContainsKey(i)) continue;
					if(VarAddrTable[i] == actualAddress) {
						actualVar = i;
						break;
					}
				}
				SetValueToVirtualMemory(actualAddress, (byte)VarMemSizeTable[(int)actualVar], value);
			} else {
				SetValueToVirtualMemory(VarAddrTable[(int)binCodeValue], byteSize, value);
			}
		}

		/// <summary>
		/// Get the actual value specified, where it could be an immediate, variable, pointer
		/// </summary>
		/// <param name="binCodeValue">The value specified in our byte code</param>
		/// <param name="opType">The opeartion type of the binCodeValue</param>
		/// <returns></returns>
		private long GetActualValueByType(long binCodeValue, byte opType) {
			byte byteSize = GetByteSizeFromOperationType(opType);
			if(opType == TYPE_IMM) return binCodeValue;
			if(opType == TYPE_PTR) {
				// Get the value of the variable first as this value is the address pointer of the actual virtual memory space we need to refer to
				int variableAddress = VarAddrTable[(int)binCodeValue];
				int actualAddress = (int)GetValueFromVirtualMemory(variableAddress, byteSize);

				// What's the destination byte size? (the byte size of the actual variable that is pointed by this pointer var)
				// Which variable actually holds this "actualAddress"
				long actualVar = 0;
				/*
				foreach(KeyValuePair<long, int> item in VarAddrTable) {
					if(item.Value == actualAddress) {
						actualVar = item.Key;
						break;
					}
				}*/

				// Normally, we reference variables created after all static variables, so it may be quicker to search the variable list from the bottom of our list
				//int lastAddress = (int)VarAddrTable.Keys.Last();
				int lastAddress = VarAddrTable.Count() - 1;
				for(int i = lastAddress; i >= 0; i--) {
					//if (!VarAddrTable.ContainsKey(i)) continue;
					if(VarAddrTable[i] == actualAddress) {
						actualVar = i;
						break;
					}
				}

				binCodeValue = GetValueFromVirtualMemory(actualAddress, (byte)VarMemSizeTable[(int)actualVar]);
			} else {
				binCodeValue = GetValueFromVirtualMemory(VarAddrTable[(int)binCodeValue], byteSize);
			}
			return binCodeValue;
		}

		/// <summary>
		/// Run program
		/// </summary>
		/// <param name="runStep">How many program steps to run: set to -1 to just run the entire code</param>
		/// <param name="pc">Program counter</param>
		public void Run(int runStep = -1, int programCounter = 0) {
			pc = programCounter;
			while(true) {
				bool isSkipProgramCount = false;
				// The first byte is the command
				byte cmd = BinCode[pc];
				// The second byte is the operation type
				byte opType = BinCode[pc + 1];
				// The third byte (consecutive byte(s)) exists only if type is not equal to TYPE_NULL
				long val = 0;
				if(opType != TYPE_NULL) val = GetBinCodeValue(pc + MAX_OPCODE_BYTE_SIZE, MaxOpcodeValueSize, BinCode);

				switch(cmd) {
					case VAR: UseVar = val; UseVarOpType = opType; break;
					case VMC: VirtualMachineCall(GetActualValueByType(val, opType)); break;
					case MOV: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(val, opType)); break;
					case JMP: pc = (int)GetActualValueByType(val, opType); isSkipProgramCount = true; break;
					case JMS: SubReturnIndexStack.Add(pc + MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize); pc = (int)GetActualValueByType(val, opType); isSkipProgramCount = true; break;
					case RET: pc = SubReturnIndexStack[SubReturnIndexStack.Count - 1]; SubReturnIndexStack.RemoveAt(SubReturnIndexStack.Count - 1); isSkipProgramCount = true; break;
					case ADD: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) + GetActualValueByType(val, opType)); break;
					case SUB: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) - GetActualValueByType(val, opType)); break;
					case MLT: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) * GetActualValueByType(val, opType)); break;
					case DIV: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) / GetActualValueByType(val, opType)); break;
					case MOD: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) % GetActualValueByType(val, opType)); break;
					case SHL: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) << (int)GetActualValueByType(val, opType)); break;
					case SHR: SetActualValueByType(UseVar, UseVarOpType, GetActualValueByType(UseVar, UseVarOpType) >> (int)GetActualValueByType(val, opType)); break;
					case CAN: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)) && Convert.ToBoolean(GetActualValueByType(val, opType)))); break;
					case COR: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)) || Convert.ToBoolean(GetActualValueByType(val, opType)))); break;
					// NOTE: pc + 2 + byteSize => [The index of next code], and [The index of next code] + 1 => The opType of next code
					case SKE: if(GetActualValueByType(UseVar, UseVarOpType) == GetActualValueByType(val, opType)) pc += GetByteSizeFromOperationType(BinCode[pc + MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize + 1]) + MAX_OPCODE_BYTE_SIZE; break;
					case AND: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)) & Convert.ToBoolean(GetActualValueByType(val, opType)))); break;
					case LOR: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)) | Convert.ToBoolean(GetActualValueByType(val, opType)))); break;
					case XOR: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)) ^ Convert.ToBoolean(GetActualValueByType(val, opType)))); break;
					case PSH: CommonValueStack.Add(GetActualValueByType(val, opType)); break;
					case POP: SetActualValueByType(val, opType, CommonValueStack[CommonValueStack.Count - 1]); CommonValueStack.RemoveAt(CommonValueStack.Count - 1); break;
					case LEQ: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) == GetActualValueByType(val, opType))); break;
					case LNE: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) != GetActualValueByType(val, opType))); break;
					case LSE: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) <= GetActualValueByType(val, opType))); break;
					case LGE: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) >= GetActualValueByType(val, opType))); break;
					case LGS: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) < GetActualValueByType(val, opType))); break;
					case LGG: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(GetActualValueByType(UseVar, UseVarOpType) > GetActualValueByType(val, opType))); break;
					// NOTE: For UMN, UPL, UNT, BUC, there are no values following after this operand because these commands directly manipulate on the currently selected variable (with VAR command),
					// Hence, we only need to increment our PC with just the opcode byte size
					case UMN: SetActualValueByType(UseVar, UseVarOpType, -GetActualValueByType(UseVar, UseVarOpType)); pc += MAX_OPCODE_BYTE_SIZE; isSkipProgramCount = true; break;
					case UPL: SetActualValueByType(UseVar, UseVarOpType, +GetActualValueByType(UseVar, UseVarOpType)); pc += MAX_OPCODE_BYTE_SIZE; isSkipProgramCount = true; break;
					case UNT: SetActualValueByType(UseVar, UseVarOpType, Convert.ToInt64(!Convert.ToBoolean(GetActualValueByType(UseVar, UseVarOpType)))); pc += MAX_OPCODE_BYTE_SIZE; isSkipProgramCount = true; break;
					case BUC: SetActualValueByType(UseVar, UseVarOpType, ~GetActualValueByType(UseVar, UseVarOpType)); pc += MAX_OPCODE_BYTE_SIZE; isSkipProgramCount = true; break;
				}
				// Point to next program code index
				if(!isSkipProgramCount) pc += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize;
				if(pc >= BinCode.Count) break;
				if(runStep == -1) continue;
				if(--runStep == 0) break;
			}
		}

		/// <summary>
		/// Pops one value from the common value stack
		/// </summary>
		/// <returns>long type value located in the last index of the stack</returns>
		protected long PopValueFromStack() { long retValue = CommonValueStack[CommonValueStack.Count - 1]; CommonValueStack.RemoveAt(CommonValueStack.Count - 1); return retValue; }
		/// <summary>
		/// Pushes one value to the common value stack
		/// </summary>
		/// <param name="value"></param>
		protected void PushValueToStack(long value) { CommonValueStack.Add(value); }

		/// <summary>Releases memory that was allocated in the Virtual Memory heap</summary>
		/// <param name="addressToRelease"></param>
		/// <param name="numBytesToRelease"></param>
		public void ReleaseMemory(int addressToRelease, int numBytesToRelease) {
			// Check if we have any variables added inside our VarAddrTable
			if(VarAddrTable.Contains(addressToRelease)) {
				int varID = 0;
				for(int i = 0; i < VarAddrTable.Count; i++) {
					if(VarAddrTable[i] == addressToRelease) {
						varID = i;
						break;
					}
				}
				// Get the variable ID (i.e. get the key from the value in the VarAddrTable dictionary)
				//var varID = VarAddrTable.FirstOrDefault(x => x.Value == addressToRelease).Key;

				// We need to update the reference addresses of all static pointers because we will remove a section in our virtual memory
				for(int i = 0; i < StaticInstanceObjectList.Count; i++) {
					int varToCheck = StaticInstanceObjectList[i];
					int varVirtualMemoryAddress = VarAddrTable[varToCheck];
					int varSize = VarMemSizeTable[varToCheck];
					long pointingAtAddress = GetValueFromVirtualMemory(varVirtualMemoryAddress, (byte)varSize);
					if(pointingAtAddress >= addressToRelease + numBytesToRelease) {
						SetValueToVirtualMemory(varVirtualMemoryAddress, (byte)varSize, pointingAtAddress - numBytesToRelease);
					}
				}

				// We also to update the reference addresses of all static pointers because we will remove a section in our virtual memory
				for(int i = 0; i < StaticInstanceObjectList.Count; i++) {
					int varToCheck = StaticInstanceObjectList[i];
					int varVirtualMemoryAddress = VarAddrTable[varToCheck];
					int varSize = VarMemSizeTable[varToCheck];
					long pointingAtAddress = GetValueFromVirtualMemory(varVirtualMemoryAddress, (byte)varSize);
					if(pointingAtAddress >= addressToRelease + numBytesToRelease) {
						SetValueToVirtualMemory(varVirtualMemoryAddress, (byte)varSize, pointingAtAddress - numBytesToRelease);
					}
				}

				// No variables are pointing to this address, so remove it
				VirtualMemory.RemoveRange(addressToRelease, numBytesToRelease);

				// Remove variable references from our VarAddrTable, for the NUMBER of bytes allocated
				int numBytesRemoved = numBytesToRelease;
				//int varIDToRemove = varID;
				while(numBytesRemoved != 0) {
					numBytesRemoved -= VarMemSizeTable[varID];
					VarAddrTable.RemoveAt(varID);
					VarMemSizeTable.RemoveAt(varID);
				}
				/*
				while(numBytesRemoved != 0) {
					numBytesRemoved -= VarMemSizeTable[varIDToRemove];
					VarAddrTable.Remove(varIDToRemove);
					VarMemSizeTable.Remove(varIDToRemove);
					// If we still have variables to remove, find the next one to be removed
					if(numBytesRemoved != 0) {
						while(VarAddrTable.ContainsKey(++varIDToRemove) == false) ;
					}
				}*/

				/*
				// Get the maximum address variable ID value so we can set the varIDCounter to point to the next variable ID to be created
				List<long> tmpList = new List<long>();
				foreach(var kvpVarAddrTable in VarAddrTable) tmpList.Add(kvpVarAddrTable.Key);
				tmpList.Sort();
				varIDCounter = tmpList[tmpList.Count - 1] + 1;
				*/

				// Update 2026/01/02 (ChatGPT bug fix)
				if(MallocAddressSizeTable.ContainsKey(addressToRelease)) MallocAddressSizeTable.Remove(addressToRelease);
				//if(MallocAddressSizeTable.ContainsKey(varID)) MallocAddressSizeTable.Remove(varID);

				// Update MallocAddressSizeTable address that is beyond the current address that we deleted
				Dictionary<long, int> newMallocAddressSizeTable = new Dictionary<long, int>();
				foreach(var kvpMallocAddressSizeTable in MallocAddressSizeTable) {
					long newKey = kvpMallocAddressSizeTable.Key;
					int currentValue = kvpMallocAddressSizeTable.Value;
					if(kvpMallocAddressSizeTable.Key > addressToRelease) {
						newMallocAddressSizeTable.Add(newKey - numBytesToRelease, currentValue);
					} else if(kvpMallocAddressSizeTable.Key < addressToRelease) {
						newMallocAddressSizeTable.Add(newKey, currentValue);
					}
				}
				MallocAddressSizeTable = newMallocAddressSizeTable;
			} else {
				// No variables are pointing to this address, so remove it
				VirtualMemory.RemoveRange(addressToRelease, numBytesToRelease);
			}
		}

		/*
		/// <summary>
		/// Run a quick garbage collection against instance variables
		/// This GC will only release memory for direct instances - meaning that it will not release anything that the newly created instance has allocated again
		/// </summary>
		/// <param name="maxSearchDepth">How many memory allocation table we should search into for</param>
		public void PerformGarbageCollection(int maxSearchDepth = 5)
		{
			if (MallocAddressSizeTable.Count == 0) return;
			// Counts how many elements we have already searched for
			int searchCnt = 0;
		RestartGCSearch:
			// For each allocated address...
			foreach (var kvp in MallocAddressSizeTable)
			{
				// Determine if we should search for another address
				// The default count is maxSearchDepth - as searching the entire memory will have high running cost and may slow down the code execution
				if (++searchCnt > maxSearchDepth) break;

				bool isAddressPointed = false;
				// Check our static instance object if they point to the allocated address...
				for (int i = 0; i < StaticInstanceObjectList.Count; i++)
				{
					int varToCheck = StaticInstanceObjectList[i];
					int varVirtualMemoryAddress = VarAddrTable[varToCheck];
					int varSize = VarMemSizeTable[varToCheck];
					long pointingAtAddress = GetValueFromVirtualMemory(varVirtualMemoryAddress, (byte)varSize);
					if (pointingAtAddress == kvp.Key)
					{
						isAddressPointed = true;
						break;
					}
				}
				if (isAddressPointed) continue;


				// Check to see if any of the newely created (dynamic) instances are pointing to the allocated address
				int lastAddress = (int)VarAddrTable.Keys.Last();
				for (int i = numDeclaredVars; i <= lastAddress; i++)
				{
					if (!VarAddrTable.ContainsKey(i)) continue;
					int varVirtualMemoryAddress = VarAddrTable[i];
					int varSize = VarMemSizeTable[i];

					long pointingAtAddress = GetValueFromVirtualMemory(varVirtualMemoryAddress, (byte)varSize);
					if (pointingAtAddress == kvp.Key)
					{
						isAddressPointed = true;
						break;
					}
				}
				if (isAddressPointed) continue;

				int varIndexToRemoveFrom = 0;
				long removeUpToAddress = (kvp.Key + kvp.Value);
				bool isFoundTargetVar = false;
				bool isCompletedAddingRemovalVars = false;
				List<long> keyToRemoveFromVarAddrTable = new List<long>();
				List<long> keyToUpdateValueForVarAddrTable = new List<long>();
				// Get the variable that we need to remove
				foreach (var kvpVarAddrTable in VarAddrTable)
				{
					if (isCompletedAddingRemovalVars)
					{
						// Here, we just loop through our dictionary's memory address (value) until the end to store the items that we need to update its value
						keyToUpdateValueForVarAddrTable.Add(kvpVarAddrTable.Key);
						continue;
					}
					// Search for the variables address that matches our memory allocation stack's target address to remove
					if (kvpVarAddrTable.Value == kvp.Key)
					{
						isFoundTargetVar = true;
					}
					if (isFoundTargetVar)
					{
						if (removeUpToAddress == kvpVarAddrTable.Value)
						{
							// Now, proceeding address for the variables will change because the preceeding ones are deleted
							// We will subtract the address from the remaining addresses so it has the correct address in our virtual memory
							// Therefore, we will store the keys that needs to be updated here
							keyToUpdateValueForVarAddrTable.Add(kvpVarAddrTable.Key);
							isCompletedAddingRemovalVars = true;
							continue;
						}
						keyToRemoveFromVarAddrTable.Add(kvpVarAddrTable.Key);
					}
					else
					{
						varIndexToRemoveFrom++;
					}
				}

				// Now, proceeding address for the variables will change because the preceeding ones are deleted
				// We will subtract the address from the remaining addresses so it has the correct address in our virtual memory
				for (int i = 0; i < keyToUpdateValueForVarAddrTable.Count; i++)
				{
					VarAddrTable[keyToUpdateValueForVarAddrTable[i]] -= (int)kvp.Value;
				}

				// We need to update the reference addresses of all static pointers because we've just removed a section in our virtual memory
				for(int i = 0; i < StaticInstanceObjectList.Count; i++) {
					int varToCheck = StaticInstanceObjectList[i];
					int varVirtualMemoryAddress = VarAddrTable[varToCheck];
					int varSize = VarMemSizeTable[varToCheck];
					long pointingAtAddress = GetValueFromVirtualMemory(varVirtualMemoryAddress, (byte)varSize);
					if(pointingAtAddress >= kvp.Key + kvp.Value) {
						SetValueToVirtualMemory(varVirtualMemoryAddress, (byte)varSize, pointingAtAddress - kvp.Value);
					}
				}

				// If no variables are pointing to this address, remove it
				VirtualMemory.RemoveRange((int)kvp.Key, kvp.Value);

				// Remove variable references from our VarAddrTable
				for (int i = 0; i < keyToRemoveFromVarAddrTable.Count; i++)
				{
					VarAddrTable.Remove(keyToRemoveFromVarAddrTable[i]);
					VarMemSizeTable.Remove(keyToRemoveFromVarAddrTable[i]);
				}

				// Get the maximum address variable ID value so we can set the varIDCounter to point to the next variable ID to be created
				List<long> tmpList = new List<long>();
				foreach(var kvpVarAddrTable in VarAddrTable) tmpList.Add(kvpVarAddrTable.Key);
				tmpList.Sort();
				varIDCounter = tmpList[tmpList.Count - 1] + 1;

				MallocAddressSizeTable.Remove(kvp.Key);

				// Update MallocAddressSizeTable address that is beyond the current address that we deleted
				Dictionary<long, int> newMallocAddressSizeTable = new Dictionary<long, int>();
				foreach (var kvpMallocAddressSizeTable in MallocAddressSizeTable)
				{
					long newKey = kvpMallocAddressSizeTable.Key;
					int currentValue = kvpMallocAddressSizeTable.Value;
					if (kvpMallocAddressSizeTable.Key > kvp.Key)
					{
						newMallocAddressSizeTable.Add(newKey - (int)kvp.Value, currentValue);
					}
					else if (kvpMallocAddressSizeTable.Key < kvp.Key)
					{
						newMallocAddressSizeTable.Add(newKey, currentValue);
					}
				}
				MallocAddressSizeTable = newMallocAddressSizeTable;

				// Update MallocAddressSizeTable address that is beyond the current address that we deleted
				// Reset and start again searching for garbages...
				goto RestartGCSearch;
			}
		}
*/
		/// <summary>Generate string object from the Virtual Memory address</summary>
		/// <param name="stringStartAddress">The first address of the beginning of string</param>
		/// <returns>string object generated from the VM address</returns>
		protected string GenerateStringFromAddress(int stringStartAddress) {
			string text = "";
			for(int i = stringStartAddress; i < VirtualMemory.Count; i += 2) {
				char c = (char)GetValueFromVirtualMemory(i, 2);
				if(c == '\0') break;
				text += c.ToString();
			}
			return text;
		}

		/// <summary>
		/// Calls a platform dependent function
		/// Here, you will write your own functions and call them to perform various s
		/// NOTE: callID from 0 to 255 are preserved by the VM
		/// </summary>
		/// <param name="callID">The ID to identify which function you want to call</param>
		protected virtual void VirtualMachineCall(long callID) {
			switch(callID) {
				case VMC_PRINT:
					string str = GenerateStringFromAddress((int)PopValueFromStack());
					Console.WriteLine(str);
					break;
				case VMC_MALLOC:
					int bytesToAllocate = (int)PopValueFromStack();
					int newVirtualMemoryAddressStartIndex = VirtualMemory.Count;
					// Create new virtual memory entries for the number of bytes are asked to create
					for(int i = 0; i < bytesToAllocate; i++) VirtualMemory.Add(0);
					// Get the maximum address variable ID value so we can point to the next variable ID to be created
					//List<long> tmpList = new List<long>();
					//foreach(var kvpVarAddrTable in VarAddrTable) tmpList.Add(kvpVarAddrTable.Key);
					//tmpList.Sort();
					//long nextVarID = tmpList[tmpList.Count - 1] + 1;
					// Add the variable to our variable address table
					VarAddrTable.Add(newVirtualMemoryAddressStartIndex);
					// Add the variable to our variable size table
					VarMemSizeTable.Add(bytesToAllocate);
					// Point to next new variable ID
					//varIDCounter++;
					// Set newly created virtual memory address to the currently selected variable (i.e. variable selected by using the VAR command)
					SetActualValueByType(UseVar, UseVarOpType, VirtualMemory.Count - bytesToAllocate);
					break;
				case VMC_REG_MALLOC_GROUP_SIZE:
					int totalMemBlockSize = (int)PopValueFromStack();
					int startAddress = (int)PopValueFromStack();
					MallocAddressSizeTable.Add(startAddress, totalMemBlockSize);
					break;
				case VMC_RUN_GC:
					//PerformGarbageCollection();
					break;
				case VMC_SET_FLOAT_MULTBY_FACTOR: DecPtMltFactor = (double)PopValueFromStack(); break;
				case VMC_FREE:
					int numBytesToRelease = (int)PopValueFromStack();
					int addressToRelease = (int)PopValueFromStack();
					ReleaseMemory(addressToRelease, numBytesToRelease);
					break;
				case VMC_SIZEOF_ALLOCMEM:
					int instanceAddress = (int)PopValueFromStack();
					if(MallocAddressSizeTable.ContainsKey(instanceAddress)) {
						PushValueToStack(MallocAddressSizeTable[instanceAddress]);
					}
					break;
				case VMC_FILE_OPEN:
					// Set file to be opened for reading/writing
					fileStreamFileName = GenerateStringFromAddress((int)PopValueFromStack());
					PushValueToStack(File.Exists(fileStreamFileName) ? 1 : 0);
					break;
				case VMC_FILE_CLEAR_BUFFER:
					fileStreamBuffer.Clear();
					break;
				case VMC_FILE_APPEND_VM_BYTES:
					int numBytesToWrite = (int)PopValueFromStack();
					int startVMAddress = (int)PopValueFromStack();
					for(int i = startVMAddress; i < startVMAddress + numBytesToWrite; i++) {
						fileStreamBuffer.Add(VirtualMemory[i]);
					}
					break;
				case VMC_FILE_SAVE:
					try {
						File.WriteAllBytes(fileStreamFileName, fileStreamBuffer.ToArray());
						PushValueToStack(1);
					} catch {
						PushValueToStack(0);
					}
					break;
				case VMC_FILE_LOAD:
					try {
						fileStreamBuffer.AddRange(File.ReadAllBytes(fileStreamFileName).ToList<byte>());
						PushValueToStack(1);
					} catch {
						PushValueToStack(0);
					}
					break;
				case VMC_FILE_READ_VM_BYTES:
					int numBytesToRead = (int)PopValueFromStack();
					int startStreamAddress = (int)PopValueFromStack();
					int writeStartVMAddress = (int)PopValueFromStack();
					for(int i = startStreamAddress; i < startStreamAddress + numBytesToRead; i++) {
						VirtualMemory[writeStartVMAddress++] = fileStreamBuffer[i];
					}
					break;
			}
		}
	}
}
