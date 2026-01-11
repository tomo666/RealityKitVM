using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RPN.Parser;

namespace JBGE {
	public class CompileOptions {
		public string FileName { get; set; }
		public string BinFileName { get; set; }
		public bool IsOutputIntermCode { get; set; } = false;
		public int MaxBitSize { get; set; } = 64;
		public int DecimalPointPrecision { get; set; } = 0;
	}

	public static class JBScript {

		public class Compiler {

			public static bool IsGenerateUnitTestFiles = false;

			// Commands (8 bit)
			public const byte NOP = 0x0; // No operation
			public const byte VMC = 0x1; // Virtual Machine Call
			public const byte VAR = 0x2; // Declare/Operate on a variable
			public const byte MOV = 0x3; // Assign value to pointer variable or variable
			public const byte JMP = 0x4; // Jump to label
			public const byte JMS = 0x5; // Jump to subroutine
			public const byte RET = 0x6; // Return from subroutine
			public const byte ADD = 0x7; // Add
			public const byte SUB = 0x8; // Subtract
			public const byte MLT = 0x9; // Multiply
			public const byte DIV = 0xA; // Divide
			public const byte MOD = 0xB; // Modulas
			public const byte SHL = 0xC; // Shift left
			public const byte SHR = 0xD; // Shift right
			public const byte CAN = 0xE; // Conditional Logical AND (&&)
			public const byte COR = 0xF; // Conditional Logical OR (||)
			public const byte SKE = 0x10; // Skip if equal
			public const byte AND = 0x11; // Logical AND
			public const byte LOR = 0x12; // Logical OR
			public const byte XOR = 0x13; // Exclusive OR
			public const byte PSH = 0x14; // Push to stack
			public const byte POP = 0x15; // Pop from stack
			public const byte LEQ = 0x16; // Logical Operator ==
			public const byte LNE = 0x17; // Logical Operator !=
			public const byte LSE = 0x18; // Logical Operator <=
			public const byte LGE = 0x19; // Logical Operator >=
			public const byte LGS = 0x1A; // Logical Operator <
			public const byte LGG = 0x1B; // Logical Operator >
			public const byte UMN = 0x1C; // UnaryMinus -
			public const byte UPL = 0x1D; // UnaryPlus +
			public const byte UNT = 0x1E; // UnaryNot !
			public const byte BUC = 0x1F; // Bitwise Unary Complement ~

			// Type (8 bit)
			public const byte TYPE_NULL = 0x0; // null
			public const byte TYPE_PTR = 0x1; // Operation manipulates on a pointer
			public const byte TYPE_IMM = 0x2; // Operation manipulates on an immediate value
			public const byte TYPE_UVAR8 = 0x3; // Operation manipulates on 8 bit unsigned variable
			public const byte TYPE_SVAR8 = 0x4; // Operation manipulates on 8 bit signed variable
			public const byte TYPE_UVAR16 = 0x5; // Operation manipulates on 16 bit unsigned variable
			public const byte TYPE_SVAR16 = 0x6; // Operation manipulates on 16 bit signed variable		
			public const byte TYPE_UVAR32 = 0x7; // Operation manipulates on 32 bit unsigned variable
			public const byte TYPE_SVAR32 = 0x8; // Operation manipulates on 32 bit signed variable
			public const byte TYPE_UVAR64 = 0x9; // Operation manipulates on 64 bit unsigned variable
			public const byte TYPE_SVAR64 = 0xA; // Operation manipulates on 64 bit signed variable

			public const byte TYPE_UIMM8 = 0xB; // Operation manipulates on 8 bit unsigned immediate
			public const byte TYPE_SIMM8 = 0xC; // Operation manipulates on 8 bit signed immediate
			public const byte TYPE_UIMM16 = 0xD; // Operation manipulates on 16 bit unsigned immediate
			public const byte TYPE_SIMM16 = 0xE; // Operation manipulates on 16 bit signed immediate		
			public const byte TYPE_UIMM32 = 0xF; // Operation manipulates on 32 bit unsigned immediate
			public const byte TYPE_SIMM32 = 0x10; // Operation manipulates on 32 bit signed immediate
			public const byte TYPE_UIMM64 = 0x11; // Operation manipulates on 64 bit unsigned immediate
			public const byte TYPE_SIMM64 = 0x12; // Operation manipulates on 64 bit signed immediate

			/*
			// Attributes that tells the command to operate on
			public const byte TYPE_REG_8 = 0x0; // 0000 Reg8 – Operation manipulates on a 8 bit variable
			public const byte TYPE_REG_16 = 0x1; // 0001 Reg16 – Operation manipulates on a 16 bit variable
			public const byte TYPE_REG_32 = 0x2; // 0010 Reg32 – Operation manipulates on a 32 bit variable
			public const byte TYPE_IMM_8 = 0x3; // 0100 imm8 – Operation manipulates on a 8 bit immediate value
			public const byte TYPE_IMM_16 = 0x4; // 0101 imm16 – Operation manipulates on a 16 bit immediate value
			public const byte TYPE_IMM_32 = 0x5; // 0110 imm32 – Operation manipulates on a 32 bit immediate value
			public const byte TYPE_IMM_REF = 0x6; // 1000 &imm – Reference to an immediate address
			public const byte TYPE_REG_REF = 0x7; // 1001 &Reg – Reference to a variable address

			public const byte TYPE_REG_64 = 0x8; // 0011 Reg64 – Operation manipulates on a 64 bit variable
			public const byte TYPE_IMM_64 = 0x9; // 0111 imm64 – Operation manipulates on a 64 bit immediate value
			public const byte TYPE_REG_128 = 0xA; // 1000 Reg64 – Operation manipulates on a 64 bit variable
			public const byte TYPE_IMM_128 = 0xB; // 1001 imm64 – Operation manipulates on a 64 bit immediate value
			*/

			// OpCode size (Command(1 byte) + OpType(1 byte) = 2 bytes)
			public const int MAX_OPCODE_BYTE_SIZE = 2;
			// Value always have a signed long size (the size of long may vary by underlying architecture)
			public int MaxOpcodeValueSize;

			// Define decimal point precision
			public double DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY = 10000;

			public const string PH_NOP = "NOP"; // No Operation / Label -- If no labels specified, it should be "NOP 0"
			public const string PH_VMC = "VMC"; // Virtual Machine Call

			public const string PH_VAR = "VAR"; // Use Variable
			public const string PH_VAR8 = "VAR8"; // Use 8 bit Variable
			public const string PH_VAR16 = "VAR16"; // Use 16 bit Variable
			public const string PH_VAR32 = "VAR32"; // Use 32 bit Variable
			public const string PH_VAR64 = "VAR64"; // Use 64 bit Variable

			public const string PH_REG = "REG"; // Declare/Register variable
			public const string PH_REG8 = "REG8"; // Declare/Operate on 8 bit
			public const string PH_REG16 = "REG16"; // Declare/Operate on 16 bit
			public const string PH_REG32 = "REG32"; // Declare/Operate on 32 bit
			public const string PH_REG64 = "REG64"; // Declare/Operate on 64 bit

			public const string PH_MOV = "MOV"; // Assign To Variable
			public const string PH_JMP = "JMP"; // Jump to label
			public const string PH_JMS = "JMS"; // Jump to subroutine
			public const string PH_RET = "RET"; // Return from subroutine
			public const string PH_ADD = "ADD"; // Add
			public const string PH_SUB = "SUB"; // Subtract
			public const string PH_MLT = "MLT"; // Multiply
			public const string PH_DIV = "DIV"; // Divide
			public const string PH_MOD = "MOD"; // Modulas
			public const string PH_SHL = "SHL"; // Left shift
			public const string PH_SHR = "SHR"; // Right shift
			public const string PH_CAN = "CAN"; // Conditional Logical AND (&&)
			public const string PH_COR = "COR"; // Conditional Logical OR (||)
			public const string PH_SKE = "SKE"; // Skip if equal
			public const string PH_AND = "AND"; // Logical AND
			public const string PH_LOR = "LOR"; // Logical OR
			public const string PH_XOR = "XOR"; // Exclusive OR
			public const string PH_PSH = "PSH"; // Push to stack
			public const string PH_POP = "POP"; // Pop from stack
			public const string PH_LEQ = "LEQ"; // Logical Operator ==
			public const string PH_LNE = "LNE"; // Logical Operator !=
			public const string PH_LSE = "LSE"; // Logical Operator <=
			public const string PH_LGE = "LGE"; // Logical Operator >=
			public const string PH_LGS = "LGS"; // Logical Operator <
			public const string PH_LGG = "LGG"; // Logical Operator >
																					//public const string PH_MAL = "MAL"; // Memory allocate
																					//public const string PH_DEL = "DEL"; // Memory deallocate

			public const string PH_UMN = "UMN"; // UnaryMinus -
			public const string PH_UPL = "UPL"; // UnaryPlus +
			public const string PH_UNT = "UNT"; // UnaryNot !
			public const string PH_BUC = "BUC"; // Bitwise Unary Complement ~

			public const string PH_RESV_KW_USING = "using ";
			public const string PH_RESV_KW_NAMESPACE = "namespace ";
			public const string PH_RESV_KW_CLASS = "class ";
			public const string PH_RESV_KW_STATIC = "static ";
			public const string PH_RESV_KW_STATIC_CLASS = "static_class ";
			public const string PH_RESV_KW_OVERRIDE = "override ";
			public const string PH_RESV_KW_VIRTUAL = "virtual ";
			public const string PH_RESV_KW_CONST = "const ";
			public const string PH_RESV_KW_RETURN = "return ";
			public const string PH_RESV_KW_GOTO = "goto ";
			public const string PH_RESV_KW_NEW = "new ";
			public const string PH_RESV_KW_VOID = "void ";
			public const string PH_RESV_KW_IF = "if";
			public const string PH_RESV_KW_ELSE = "else";
			public const string PH_RESV_KW_ELSEIF = "else if";
			public const string PH_RESV_KW_WHILE = "while";
			public const string PH_RESV_KW_BREAK = "break";
			public const string PH_RESV_KW_CONTINUE = "continue";
			public const string PH_RESV_KW_FOR = "for";
			public const string PH_RESV_KW_FUNCTION = "function ";
			public const string PH_RESV_KW_ENUM = "enum ";

			public const string PH_RESV_KW_FUNCTION_PTR = "function_ptr ";
			public const string PH_RESV_KW_FUNCTION_VOID = "function_void ";
			public const string PH_RESV_KW_FUNCTION_STRING = "function_string ";
			public const string PH_RESV_KW_FUNCTION_CHAR = "function_char ";
			public const string PH_RESV_KW_FUNCTION_OBJECT = "function_object ";
			public const string PH_RESV_KW_FUNCTION_BOOL = "function_bool ";
			public const string PH_RESV_KW_FUNCTION_SBYTE = "function_sbyte ";
			public const string PH_RESV_KW_FUNCTION_BYTE = "function_byte ";
			public const string PH_RESV_KW_FUNCTION_SHORT = "function_short ";
			public const string PH_RESV_KW_FUNCTION_USHORT = "function_ushort ";
			public const string PH_RESV_KW_FUNCTION_INT = "function_int ";
			public const string PH_RESV_KW_FUNCTION_UINT = "function_uint ";
			public const string PH_RESV_KW_FUNCTION_LONG = "function_long ";
			public const string PH_RESV_KW_FUNCTION_ULONG = "function_ulong ";
			public const string PH_RESV_KW_FUNCTION_FLOAT = "function_float ";
			public const string PH_RESV_KW_FUNCTION_DOUBLE = "function_double ";
			public const string PH_RESV_KW_FUNCTION_DECIMAL = "function_decimal ";

			public const string PH_RESV_KW_STRING = "string ";
			public const string PH_RESV_KW_CHAR = "char ";
			public const string PH_RESV_KW_PTR = "ptr𒀭 "; // Represents a pointer


			public const string PH_RESV_KW_PTR_VOID = "void_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_STRING = "string_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_CHAR = "char_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_OBJECT = "object_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_BOOL = "bool_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_SBYTE = "sbyte_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_BYTE = "byte_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_SHORT = "short_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_USHORT = "ushort_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_INT = "int_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_UINT = "uint_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_LONG = "long_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_ULONG = "ulong_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_FLOAT = "float_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_DOUBLE = "double_" + PH_RESV_KW_PTR;
			public const string PH_RESV_KW_PTR_DECIMAL = "decimal_" + PH_RESV_KW_PTR;



			public const string PH_RESV_KW_OBJECT = "object "; // 64 BIT
			public const string PH_RESV_KW_VAR = "var "; // 16 BIT | 32 BIT | 64 BIT | 128 BIT - Default can be changed but range is fixed
			public const string PH_RESV_KW_BOOL = "bool "; // 8 BIT | 1 or 0
			public const string PH_RESV_KW_SBYTE = "sbyte "; // 8 BIT | -128... 127
			public const string PH_RESV_KW_SHORT = "short "; // 16 BIT | -32,768 to +32,767
			public const string PH_RESV_KW_INT = "int "; // 32 BIT | -2,147,483,648 to +2,147,483,647
			public const string PH_RESV_KW_LONG = "long "; // 64 BIT | -9,223,372,036,854,775,808 to +9,223,372,036,854,775,807
			public const string PH_RESV_KW_ULONG = "ulong "; // 64 BIT | 0 to +18,446,744,073,709,551,615
			public const string PH_RESV_KW_BYTE = "byte "; // 8 BIT | 0... 255
			public const string PH_RESV_KW_USHORT = "ushort "; // 16 BIT | 0 to +65,535
			public const string PH_RESV_KW_UINT = "uint "; // 32 BIT | 0 to +4,294,967,295
			public const string PH_RESV_KW_FLOAT = "float "; // 32 BIT | ±1.5 x 10−45 ～ ±3.4 x 1038 : 有効桁数は「~6 ～ 9 桁」
			public const string PH_RESV_KW_DOUBLE = "double "; // 64 BIT | ±5.0 × 10−324 ～ ±1.7 × 10308 : 有効桁数は「~15 ～ 17 桁」
			public const string PH_RESV_KW_DECIMAL = "decimal "; // 128 BIT | ±1.0 x 10-28 ～ ±7.9228 x 1028 : 有効桁数は「28 ～ 29 桁の数字」
			public const string PH_RESV_KW_BASE = "base";
			public const string PH_RESV_KW_THIS = "this";
			public const string PH_RESV_KW_TRUE = "true";
			public const string PH_RESV_KW_FALSE = "false";
			public const string PH_RESV_KW_NULL = "null";

			public const string PH_RESV_KW_PROGRAM_INIT_POINT_ATTRIBUTE = "[DScript_Init]";
			public const string PH_RESV_KW_PROGRAM_INIT_POINT_LABEL = PH_ID + PH_LABEL + PH_ID + "@DScript_Init;";
			public const string PH_RESV_KW_PROGRAM_MAIN_LOOP_ATTRIBUTE = "[DScript_Main]";
			public const string PH_RESV_KW_PROGRAM_MAIN_LOOP_LABEL = PH_ID + PH_LABEL + PH_ID + "@DScript_Main;";

			public const string PH_RESV_KW_IASM = "__iasm";
			public const string PH_RESV_KW_IASM_FUNCTION_CALL = "DScript.VMC.__iasm";
			public const string PH_RESV_SYSFUNC_MALLOC = "DScript.VMC.Malloc";
			public const string PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE = "DScript.VMC.RegisterAllocatedMemBlockSize";
			public const string PH_RESV_SYSFUNC_SET_FLOAT_PRECISION = "DScript.VMC.SetFloatMultByFactor";

			public string[] ObsoleteKeywords = new string[] { "public ", "private ", "protected ", PH_RESV_KW_VIRTUAL, PH_RESV_KW_STATIC, PH_RESV_KW_CONST, PH_RESV_KW_ENUM };
			public string[] ClassMemberReservedKeyWords = new string[] {
			PH_RESV_KW_VOID, PH_RESV_KW_FUNCTION, PH_RESV_KW_FUNCTION_PTR, PH_RESV_KW_FUNCTION_VOID, PH_RESV_KW_FUNCTION_STRING, PH_RESV_KW_FUNCTION_CHAR, PH_RESV_KW_FUNCTION_OBJECT, PH_RESV_KW_FUNCTION_BOOL, PH_RESV_KW_FUNCTION_SBYTE, PH_RESV_KW_FUNCTION_BYTE, PH_RESV_KW_FUNCTION_SHORT,
			PH_RESV_KW_FUNCTION_USHORT, PH_RESV_KW_FUNCTION_INT, PH_RESV_KW_FUNCTION_UINT, PH_RESV_KW_FUNCTION_LONG, PH_RESV_KW_FUNCTION_ULONG, PH_RESV_KW_FUNCTION_FLOAT, PH_RESV_KW_FUNCTION_DOUBLE, PH_RESV_KW_FUNCTION_DECIMAL,
			PH_RESV_KW_STRING, PH_RESV_KW_CHAR, PH_RESV_KW_VAR, PH_RESV_KW_PTR,
			PH_RESV_KW_PTR_VOID, PH_RESV_KW_PTR_STRING, PH_RESV_KW_PTR_CHAR, PH_RESV_KW_PTR_OBJECT, PH_RESV_KW_PTR_BOOL, PH_RESV_KW_PTR_SBYTE, PH_RESV_KW_PTR_BYTE,
			PH_RESV_KW_PTR_SHORT, PH_RESV_KW_PTR_USHORT, PH_RESV_KW_PTR_INT, PH_RESV_KW_PTR_UINT, PH_RESV_KW_PTR_LONG, PH_RESV_KW_PTR_ULONG, PH_RESV_KW_PTR_FLOAT, PH_RESV_KW_PTR_DOUBLE, PH_RESV_KW_PTR_DECIMAL,
			PH_RESV_KW_OBJECT,
			PH_RESV_KW_BOOL, PH_RESV_KW_SBYTE, PH_RESV_KW_SHORT, PH_RESV_KW_INT, PH_RESV_KW_LONG, PH_RESV_KW_ULONG, PH_RESV_KW_BYTE, PH_RESV_KW_USHORT, PH_RESV_KW_UINT, PH_RESV_KW_FLOAT, PH_RESV_KW_DOUBLE, PH_RESV_KW_DECIMAL,
		};
			public string[] AllReservedKeywords = new string[] {
			PH_RESV_KW_USING, PH_RESV_KW_NAMESPACE, PH_RESV_KW_CLASS, PH_RESV_KW_STATIC_CLASS, PH_RESV_KW_OVERRIDE, PH_RESV_KW_VIRTUAL, PH_RESV_KW_CONST, PH_RESV_KW_ENUM, PH_RESV_KW_RETURN, PH_RESV_KW_GOTO, PH_RESV_KW_NEW,
			PH_RESV_KW_VOID, PH_RESV_KW_IF, PH_RESV_KW_ELSE, PH_RESV_KW_ELSEIF, PH_RESV_KW_WHILE, PH_RESV_KW_BREAK, PH_RESV_KW_CONTINUE, PH_RESV_KW_FOR,
			PH_RESV_KW_FUNCTION, PH_RESV_KW_FUNCTION_PTR, PH_RESV_KW_FUNCTION_VOID, PH_RESV_KW_FUNCTION_STRING, PH_RESV_KW_FUNCTION_CHAR, PH_RESV_KW_FUNCTION_OBJECT, PH_RESV_KW_FUNCTION_BOOL, PH_RESV_KW_FUNCTION_SBYTE, PH_RESV_KW_FUNCTION_BYTE, PH_RESV_KW_FUNCTION_SHORT,
			PH_RESV_KW_FUNCTION_USHORT, PH_RESV_KW_FUNCTION_INT, PH_RESV_KW_FUNCTION_UINT, PH_RESV_KW_FUNCTION_LONG, PH_RESV_KW_FUNCTION_ULONG, PH_RESV_KW_FUNCTION_FLOAT, PH_RESV_KW_FUNCTION_DOUBLE, PH_RESV_KW_FUNCTION_DECIMAL,
			PH_RESV_KW_STRING, PH_RESV_KW_CHAR, PH_RESV_KW_VAR, PH_RESV_KW_PTR,
			PH_RESV_KW_PTR_VOID, PH_RESV_KW_PTR_STRING, PH_RESV_KW_PTR_CHAR, PH_RESV_KW_PTR_OBJECT, PH_RESV_KW_PTR_BOOL, PH_RESV_KW_PTR_SBYTE, PH_RESV_KW_PTR_BYTE,
			PH_RESV_KW_PTR_SHORT, PH_RESV_KW_PTR_USHORT, PH_RESV_KW_PTR_INT, PH_RESV_KW_PTR_UINT, PH_RESV_KW_PTR_LONG, PH_RESV_KW_PTR_ULONG, PH_RESV_KW_PTR_FLOAT, PH_RESV_KW_PTR_DOUBLE, PH_RESV_KW_PTR_DECIMAL,
			PH_RESV_KW_OBJECT,
			PH_RESV_KW_BOOL, PH_RESV_KW_SBYTE, PH_RESV_KW_SHORT, PH_RESV_KW_INT, PH_RESV_KW_LONG, PH_RESV_KW_BYTE, PH_RESV_KW_USHORT, PH_RESV_KW_UINT, PH_RESV_KW_FLOAT, PH_RESV_KW_DOUBLE, PH_RESV_KW_DECIMAL,
			PH_RESV_KW_BASE, PH_RESV_KW_THIS, PH_RESV_KW_TRUE, PH_RESV_KW_FALSE, PH_RESV_KW_NULL, PH_RESV_KW_IASM
		};
			public string[] AllPrimitiveTypePointerKeywords = new string[] {
			PH_RESV_KW_PTR,
			PH_RESV_KW_PTR_VOID, PH_RESV_KW_PTR_STRING, PH_RESV_KW_PTR_CHAR, PH_RESV_KW_PTR_OBJECT, PH_RESV_KW_PTR_BOOL, PH_RESV_KW_PTR_SBYTE, PH_RESV_KW_PTR_BYTE,
			PH_RESV_KW_PTR_SHORT, PH_RESV_KW_PTR_USHORT, PH_RESV_KW_PTR_INT, PH_RESV_KW_PTR_UINT, PH_RESV_KW_PTR_LONG, PH_RESV_KW_PTR_ULONG, PH_RESV_KW_PTR_FLOAT, PH_RESV_KW_PTR_DOUBLE, PH_RESV_KW_PTR_DECIMAL,

		};
			public string[] AllFunctionKeywords = new string[] {
			PH_RESV_KW_FUNCTION_PTR, PH_RESV_KW_FUNCTION_VOID, PH_RESV_KW_FUNCTION_STRING, PH_RESV_KW_FUNCTION_CHAR, PH_RESV_KW_FUNCTION_OBJECT, PH_RESV_KW_FUNCTION_BOOL, PH_RESV_KW_FUNCTION_SBYTE, PH_RESV_KW_FUNCTION_BYTE, PH_RESV_KW_FUNCTION_SHORT,
			PH_RESV_KW_FUNCTION_USHORT, PH_RESV_KW_FUNCTION_INT, PH_RESV_KW_FUNCTION_UINT, PH_RESV_KW_FUNCTION_LONG, PH_RESV_KW_FUNCTION_ULONG, PH_RESV_KW_FUNCTION_FLOAT, PH_RESV_KW_FUNCTION_DOUBLE, PH_RESV_KW_FUNCTION_DECIMAL
		};
			public string[] NonVariableDeclareKeywords = new string[] {
			PH_RESV_KW_USING, PH_RESV_KW_NAMESPACE, PH_RESV_KW_CLASS, PH_RESV_KW_STATIC_CLASS, PH_RESV_KW_OVERRIDE, PH_RESV_KW_VIRTUAL, PH_RESV_KW_CONST, PH_RESV_KW_ENUM, PH_RESV_KW_RETURN, PH_RESV_KW_GOTO, PH_RESV_KW_NEW, PH_RESV_KW_VOID,
			PH_RESV_KW_FUNCTION, PH_RESV_KW_FUNCTION_PTR, PH_RESV_KW_FUNCTION_VOID, PH_RESV_KW_FUNCTION_STRING, PH_RESV_KW_FUNCTION_CHAR, PH_RESV_KW_FUNCTION_OBJECT, PH_RESV_KW_FUNCTION_BOOL, PH_RESV_KW_FUNCTION_SBYTE, PH_RESV_KW_FUNCTION_BYTE, PH_RESV_KW_FUNCTION_SHORT,
			PH_RESV_KW_FUNCTION_USHORT, PH_RESV_KW_FUNCTION_INT, PH_RESV_KW_FUNCTION_UINT, PH_RESV_KW_FUNCTION_LONG, PH_RESV_KW_FUNCTION_ULONG, PH_RESV_KW_FUNCTION_FLOAT, PH_RESV_KW_FUNCTION_DOUBLE, PH_RESV_KW_FUNCTION_DECIMAL,
			PH_RESV_KW_IF, PH_RESV_KW_ELSE, PH_RESV_KW_ELSEIF, PH_RESV_KW_WHILE, PH_RESV_KW_BREAK, PH_RESV_KW_CONTINUE, PH_RESV_KW_FOR, PH_RESV_KW_TRUE, PH_RESV_KW_FALSE, PH_RESV_KW_NULL, PH_RESV_KW_IASM
		};
			public string[] NonMethodKeywords = new string[] {
			PH_RESV_KW_IF, PH_RESV_KW_ELSEIF, PH_RESV_KW_ELSE, PH_RESV_KW_WHILE, PH_RESV_KW_BREAK, PH_RESV_KW_CONTINUE, PH_RESV_KW_FOR, PH_RESV_KW_TRUE, PH_RESV_KW_FALSE, PH_RESV_KW_NULL, PH_RESV_KW_IASM
		};
			// Keywords that has round brackets and can hold scope brackets (curly brackets) that can be ommitted
			public string[] RoundBracketScopeHolderKeywords = new string[] {
			PH_RESV_KW_IF, PH_RESV_KW_ELSEIF, PH_RESV_KW_WHILE, PH_RESV_KW_FOR
		};

			public const string PH_ID = "𒀭";
			public const string PH_POINTER = "PH_PTR";
			public const string PH_ADDRESS = "PH_ADDRESS";
			public const string PH_EXP_STR = "PH_EXP_STR";
			public const string PH_STR = "PH_STR";
			public const string PH_CHAR = "PH_CHAR";
			public const string PH_NEW = "PH_NEW";
			public const string PH_LABEL = "PH_LBL";
			public const string PH_LEFTEXP_RESULT = "PH_LEFTEXP";
			public const string PH_ESC_DBL_QUOTE = PH_ID + "PH_ESC_DBL_QUOTE" + PH_ID;
			public const string PH_ESC_SNGL_QUOTE = PH_ID + "PH_ESC_SNGL_QUOTE" + PH_ID;

			// These are used for temporary calculation variables used in VM
			public const string REG_NULL = "REG_NULL";
			public const string REG_SVAR0 = "REG_SVAR0";
			public const string REG_SVAR1 = "REG_SVAR1";
			public const string REG_UVAR0 = "REG_UVAR0";
			public const string REG_UVAR1 = "REG_UVAR1";

			public const string REG_VAR_BYTE0 = "REG_VAR_BYTE0";
			public const string REG_VAR_BYTE1 = "REG_VAR_BYTE1";
			public const string REG_VAR_SBYTE0 = "REG_VAR_SBYTE0";
			public const string REG_VAR_SBYTE1 = "REG_VAR_SBYTE1";
			public const string REG_VAR_SHORT0 = "REG_VAR_SHORT0";
			public const string REG_VAR_SHORT1 = "REG_VAR_SHORT1";
			public const string REG_VAR_USHORT0 = "REG_VAR_USHORT0";
			public const string REG_VAR_USHORT1 = "REG_VAR_USHORT1";
			public const string REG_VAR_INT0 = "REG_VAR_INT0";
			public const string REG_VAR_INT1 = "REG_VAR_INT1";
			public const string REG_VAR_UINT0 = "REG_VAR_UINT0";
			public const string REG_VAR_UINT1 = "REG_VAR_UINT1";
			public const string REG_VAR_LONG0 = "REG_VAR_LONG0";
			public const string REG_VAR_LONG1 = "REG_VAR_LONG1";
			public const string REG_VAR_ULONG0 = "REG_VAR_ULONG0";
			public const string REG_VAR_ULONG1 = "REG_VAR_ULONG1";
			public const string REG_VAR_FLOAT0 = "REG_VAR_FLOAT0";
			public const string REG_VAR_FLOAT1 = "REG_VAR_FLOAT1";
			public const string REG_VAR_DOUBLE0 = "REG_VAR_DOUBLE0";
			public const string REG_VAR_DOUBLE1 = "REG_VAR_DOUBLE1";
			public const string REG_VAR_DECIMAL0 = "REG_VAR_DECIMAL0";
			public const string REG_VAR_DECIMAL1 = "REG_VAR_DECIMAL1";

			// Targetted for x bit processor
			public int TARGET_ARCH_BIT_SIZE = 64;
			public int CHAR_BIT_SIZE = 16;
			public int BYTE_BIT_SIZE = 8;
			public int SHORT_BIT_SIZE = 16;
			public int INT_BIT_SIZE = 32;
			public int LONG_BIT_SIZE = 64;
			public int FLOAT_BIT_SIZE = 32;
			public int DOUBLE_BIT_SIZE = 64;
			public int DECIMAL_BIT_SIZE = 64; // Could be 128 bit, if supported


			// The string length to use when generating random alphabetic strings
			public const int RND_ALPHABET_STRING_LEN_MAX = 10;
			// Temporary variable identifier to attach to the randomized string (we will use this identifier to differentiate between temporary variables and user-defined variables)
			public const string TmpVarIdentifier = "RQ20131212DSRNDID";

			// List of operators (order matters!)
			public string[] Operators = new string[] { "UnaryMinus", "UnaryPlus", "UnaryNot", "BitwiseUnaryComplement", "＋", "—", "<=", ">=", "<", ">", "==", "!=", "&&", "||", "&", "|", "^", "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "<<", ">>", "=", "!", "~", "--", "++", "-", "+", "*", "/", "%", "." };

			// List of code separator tokens
			private string SeparatorPattern = @"([＋—*()\^\/{}=;,:<>!&\|%~]|(?<!E)[\+\-]|" + PH_RESV_KW_NEW + ")";
			// Stores all literals
			Dictionary<string, string> StringLiteralList = new Dictionary<string, string>();
			Dictionary<string, string> CharLiteralList = new Dictionary<string, string>();

			// Stores the actual member size list of a non-static class
			private Dictionary<string, List<int>> ActualClassMemberVarSizeList = new Dictionary<string, List<int>>();
			// Stores the actual var type of a variable before converted into a pointer type
			private Dictionary<string, string> ActualClassMemberPointerVarTypeList = new Dictionary<string, string>();
			// Stores all the "using" namespace declarations
			private List<string> AllUsingNamespaceDeclarations = new List<string>();

			#region Helpers
			private void ShowLog(string text) {
				Console.WriteLine(text);
			}
			/// <summary>
			/// Generates a random string with a given size.
			/// </summary>
			/// <param name="size">String length of random string</param>
			/// <param name="lowerCase">Whether to include lower case or not</param>
			/// <param name="additionalIdentifier">Additional string that can be added to the end of string</param>
			/// <returns></returns>
			public static string RandomString(int size, bool lowerCase, string additionalIdentifier) {
				var builder = new StringBuilder(size);

				// Unicode/ASCII Letters are divided into two blocks
				// (Letters 65–90 / 97–122):
				// The first group containing the uppercase letters and
				// the second group containing the lowercase.  

				// char is a single Unicode character  
				char offset = lowerCase ? 'a' : 'A';
				const int lettersOffset = 26; // A...Z or a..z: length=26  

				Random _random = new Random();

				for(var i = 0; i < size; i++) {
					var @char = (char)_random.Next(offset, offset + lettersOffset);
					builder.Append(@char);
				}

				string ret = lowerCase ? builder.ToString().ToLower() : builder.ToString();
				ret += additionalIdentifier;

				return ret;
			}
			/// <summary>
			/// Cleans up string by removing sequential spaces, tabs, linebreaks
			/// </summary>
			/// <param name="text">Context string</param>
			/// <returns>Cleaned string</returns>
			private string Cleanup(string text) {
				text = ReplaceSequentialRepStringToSingle(text, "\r\n");
				text = ReplaceSequentialRepStringToSingle(text, " ");
				text = ReplaceSequentialRepStringToSingle(text, "\t");
				text = text.Trim();
				return text;
			}
			/// <summary>
			/// Replaces sequential occurance of a string to a single string
			/// </summary>
			/// <param name="text">Context string</param>
			/// <param name="replaceToSingleString">Specify the 'single' string you are after - e.g. if replacing '\t\t', then specify a single '\t'</param>
			/// <returns></returns>
			public string ReplaceSequentialRepStringToSingle(string text, string replaceToSingleString) {
				int oldLength;
				do {
					oldLength = text.Length;
					text = text.Replace(replaceToSingleString + replaceToSingleString, replaceToSingleString);
				} while(text.Length != oldLength);
				return text;
			}
			/// <summary>Replace first occurance of string</summary>
			/// <param name="text">Context string</param>
			/// <param name="search">String to search for</param>
			/// <param name="replace">String to be replaced</param>
			/// <returns></returns>
			public static string ReplaceFirstOccurance(string text, string search, string replace) {
				int pos = text.IndexOf(search);
				if(pos < 0) return text;
				return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
			}
			#endregion

			public byte[] Compile(CompileOptions co, bool isWriteToBinFile = true) {
				if(!File.Exists(co.FileName)) { ShowLog("File " + co.FileName + " does not exist."); return null; }

				// Define/Set the bit size depending on the architecture set in the commandline
				if(co.MaxBitSize == 16) {
					TARGET_ARCH_BIT_SIZE = 16;
					CHAR_BIT_SIZE = 16;
					BYTE_BIT_SIZE = 8;
					SHORT_BIT_SIZE = 16;
					INT_BIT_SIZE = 16;
					LONG_BIT_SIZE = 16;
					FLOAT_BIT_SIZE = 16;
					DOUBLE_BIT_SIZE = 16;
					DECIMAL_BIT_SIZE = 16;
				} else if(co.MaxBitSize == 32) {
					TARGET_ARCH_BIT_SIZE = 32;
					CHAR_BIT_SIZE = 16;
					BYTE_BIT_SIZE = 8;
					SHORT_BIT_SIZE = 16;
					INT_BIT_SIZE = 32;
					LONG_BIT_SIZE = 32;
					FLOAT_BIT_SIZE = 32;
					DOUBLE_BIT_SIZE = 32;
					DECIMAL_BIT_SIZE = 32;
				} else if(co.MaxBitSize == 64) {
					TARGET_ARCH_BIT_SIZE = 64;
					CHAR_BIT_SIZE = 16;
					BYTE_BIT_SIZE = 8;
					SHORT_BIT_SIZE = 16;
					INT_BIT_SIZE = 32;
					LONG_BIT_SIZE = 64;
					FLOAT_BIT_SIZE = 32;
					DOUBLE_BIT_SIZE = 64;
					DECIMAL_BIT_SIZE = 64;
				} else if(co.MaxBitSize == 128) {
					TARGET_ARCH_BIT_SIZE = 128;
					CHAR_BIT_SIZE = 16;
					BYTE_BIT_SIZE = 8;
					SHORT_BIT_SIZE = 16;
					INT_BIT_SIZE = 32;
					LONG_BIT_SIZE = 64;
					FLOAT_BIT_SIZE = 32;
					DOUBLE_BIT_SIZE = 64;
					DECIMAL_BIT_SIZE = 128;
				}

				MaxOpcodeValueSize = co.MaxBitSize / 8;

				DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY = (double)(Math.Pow(10, co.DecimalPointPrecision));

				ShowLog("Compiling " + Path.GetFileName(co.FileName));

				// Read script file to be compiled
				string code = File.ReadAllText(co.FileName);
				// Normalize line breaks to \n
				code = code.Replace("\r\n", "\n").Replace("\r", "\n");

				ShowLog("Loading external includes...");
				// Load all codes that are included using the "using" keyword
				code = LoadExternalIncludes(Path.GetDirectoryName(co.FileName), code);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestLoadExternalIncludes.txt", code);

				ShowLog("Preprocessing... ");
				// Preprocess script code and convert them into line of codes
				string[] codes = PreProcessCleanUp(code);
				ShowLog("Converting to assembly... ");
				// Converts preprocessed intermediatary source code to Assembly language codes
				codes = ConvertIntermCodeToAsm(codes, co);
				ShowLog("Converting to binary... ");
				// Convert IASM codes to binary and write to file
				byte[] binCode = ConvertAsmToByteCode(codes);


				// Write final binary code to file
				if(isWriteToBinFile) {
					ShowLog("Generating binary file... ");
					File.WriteAllBytes(co.BinFileName, binCode);
				}

				ShowLog("Done!");

				return binCode;
			}

			#region ByteCodeConvert

			/// <summary>
			/// Converts all ASM codes to byte codes
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private byte[] ConvertAsmToByteCode(string[] codes) {
				List<byte> binCode = new List<byte>();
				// Stores the variable list as (variable name, variable type = {u|s}XX)
				Dictionary<string, string> varList = new Dictionary<string, string>();
				// Get the variables and its size in bits
				// Note that the variables are already converted to $0, $1, $2, #3, etc like in sequential format
				// NOTE: Remember that the first 3 line of codes are JMS commands to jump to the main entry point, so we need to ignore
				for(int i = 3; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					// Process the code until we find a non VAR command
					if(splitCode[0].IndexOf(PH_VAR) == -1) break;
					string bitSizeStr = splitCode[0].Substring(PH_VAR.Length + 1);
					varList.Add(splitCode[1].Replace("#", "$"), splitCode[0].Replace(PH_VAR, ""));
				}

				// Convert each line of code to binary code
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					string cmd = splitCode[0];
					string val = "";
					if(splitCode.Length != 1) val = splitCode[1];
					byte[] byteCode = ConvertSingleLineOfCodeToByteCode(cmd, val, varList);
					foreach(byte b in byteCode) binCode.Add(b);
				}

				if(IsGenerateUnitTestFiles) {
					byte[] testByteCodes = binCode.ToArray<byte>();
					File.WriteAllBytes("/Users/tomo/GitHub/jb-script/Test/TestConvertAsmToByteCode_ConvertSingleLineOfCodeToByteCode.bin", testByteCodes);
				}
				// Remove labels (NOP) and convert JMP/JMS code label references to actual line in the byte code
				byte[] newByteCodes = binCode.ToArray<byte>();
				newByteCodes = RemoveLabelIDsAndConvertJumpToBinCodeIndex(newByteCodes);

				if(IsGenerateUnitTestFiles) {
					File.WriteAllBytes("/Users/tomo/GitHub/jb-script/Test/TestConvertAsmToByteCode_RemoveLabelIDsAndConvertJumpToBinCodeIndex.bin", newByteCodes);
				}

				return newByteCodes;
			}

			/// <summary>
			/// Reads all the source code specified by the "using " keyword and merges together with our main code
			/// NOTE:
			/// This method intentionally uses PreProcessCodeBeforeLOCSeparation()
			/// to avoid false positives of "using" inside strings, enums, or comments.
			/// </summary>
			/// <param name="currentFilePath"></param>
			/// <param name="code"></param>
			/// <returns>Returns the combined code from external/nested using keyword WITHOUT any pre-processings</returns>
			public string LoadExternalIncludes(string currentFilePath, string code) {
				List<string> LoadedFileList = new List<string>();
				// tmpCode stores pre-cleaned up source code so we can search for the "using " keyword
				string tmpCode = PreProcessCodeBeforeLOCSeparation(code);
				CharLiteralList.Clear();
				StringLiteralList.Clear();

				MatchCollection mc = Regex.Matches(tmpCode, PH_RESV_KW_USING + "(.*?);");
				while(mc.Count != 0) {
					foreach(Match m in mc) {
						string usingName = m.Value.Replace(PH_RESV_KW_USING, "").Replace(";", "");
						if(!AllUsingNamespaceDeclarations.Contains<string>(usingName)) AllUsingNamespaceDeclarations.Add(usingName);
						string externFile = currentFilePath + Path.DirectorySeparatorChar + m.Value.Replace(PH_RESV_KW_USING, "").Replace(";", "") + ".cs";
						if(!File.Exists(externFile)) {
							code = code.Replace(m.Value, "");
							continue;
						}
						// Prevents loading same files
						if(!LoadedFileList.Contains(externFile)) {
							string externCode = File.ReadAllText(externFile);
							// Normalize line breaks to \n
							externCode = externCode.Replace("\r\n", "\n").Replace("\r", "\n");
							code = code.Replace(m.Value, "");
							code = externCode + code;
							LoadedFileList.Add(externFile);
						} else {
							ShowLog("Loading external include file [" + Path.GetFileName(externFile) + "]");
							code = code.Replace(m.Value, "");
						}
					}
					// Check code again to see if there's any "using " keyword left over
					CharLiteralList.Clear();
					StringLiteralList.Clear();
					ShowLog("Preprocessing with external include file...");
					tmpCode = PreProcessCodeBeforeLOCSeparation(code);
					mc = Regex.Matches(tmpCode, PH_RESV_KW_USING + "(.*?);");
				}
				CharLiteralList.Clear();
				StringLiteralList.Clear();
				return code;
			}

			/// <summary>
			/// Get the actual value (in long size) in the BinCode
			/// The value size (in bytes) is dependant on the specified MaxBitSize
			/// </summary>
			/// <param name="byteCodeList">Reference to the byte list</param>
			/// <param name="startBinCodeIndex">Start address to seek from</param>
			/// <returns></returns>
			private long GetBinCodeValue(byte[] codes, int startBinCodeIndex) {
				long value = 0;
				int addr = startBinCodeIndex;
				int byteSize = TARGET_ARCH_BIT_SIZE / 8;
				for(int i = 0; i < byteSize; i++) {
					value <<= 8;
					value |= codes[addr + i];
				}
				return value;
			}

			/// <summary>
			/// Get the byte size according to the specified architecture / MaxBitSize (maximum bit size allowed)
			/// </summary>
			/// <param name="opType">One of the operation types defined in the constants (TYPE_XXX)</param>
			/// <returns></returns>
			private int GetByteSizeFromOperationType(byte opType) {
				int byteSize = 0;
				switch(opType) {
					case TYPE_UVAR8: case TYPE_SVAR8: byteSize = 1; break;
					case TYPE_UVAR16: case TYPE_SVAR16: byteSize = 2; break;
					case TYPE_UVAR32: case TYPE_SVAR32: byteSize = 4; break;
					case TYPE_UVAR64: case TYPE_SVAR64: byteSize = 8; break;
					case TYPE_NULL: byteSize = 0; break;
					default: byteSize = TARGET_ARCH_BIT_SIZE / 8; break;
				}
				return byteSize;
			}

			/// <summary>
			/// Remove all labels and convert label ID references on JMP and JMS to absolute program code index
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private byte[] RemoveLabelIDsAndConvertJumpToBinCodeIndex(byte[] codes) {
				// Get the list of labels (label ID, code index)
				Dictionary<int, int> labelList = new Dictionary<int, int>();
				List<byte> codeList = codes.ToList<byte>();

				for(int i = 0; i < codeList.Count; i++) {
					byte cmd = codeList[i];
					byte type = codeList[i + 1];
					if(cmd != NOP) {
						i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize;
						// RET command does not take any values, so we simply subtract the value size
						if(cmd == RET || cmd == UMN || cmd == UPL || cmd == UNT || cmd == BUC) i -= MaxOpcodeValueSize;
						// Since we are incrementing i in this loop, we subtrack one here
						i--;
						continue;
					}
					if(type == TYPE_NULL) break;

					//int codeIndex = i - reducedLineOfCode;
					labelList.Add((int)GetBinCodeValue(codeList.ToArray(), i + MAX_OPCODE_BYTE_SIZE), i);
					// Remove this label (NOP) code
					for(int j = i; j < i + MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize; j++) codeList.RemoveAt(i);
					// Since we are incrementing i in this loop, we subtrack one here
					i--;
				}
				codes = codeList.ToArray();

				// For each code, replace JMP/JMS labels to actual code index
				for(int i = 0; i < codes.Length; i++) {
					byte cmd = codes[i];
					byte type = codes[i + 1];
					if(cmd != JMP && cmd != JMS) {
						i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize;
						// RET command does not take any values, so we simply subtract the value size
						if(cmd == RET || cmd == UMN || cmd == UPL || cmd == UNT || cmd == BUC) i -= MaxOpcodeValueSize;
						// Since we are incrementing i in this loop, we subtrack one here
						i--;
						continue;
					}
					int value = (int)GetBinCodeValue(codes, i + MAX_OPCODE_BYTE_SIZE);
					// Get the actual address where we need to jump to
					byte[] valBytes = BitConverter.GetBytes(labelList[value]);
					// Replace the current JMP/JMS label ID to the actual address
					for(int j = 0; j < MaxOpcodeValueSize; j++) {
						codes[i + 2 + j] = valBytes[MaxOpcodeValueSize - j - 1];
					}
					i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize;
					// Since we are incrementing i in this loop, we subtrack one here
					i--;
				}

				return codes;
			}

			/// <summary>
			/// Convert one line of IASM code to byte code
			/// NOTE: The value will be converted to the target architecture maximum size (i.e. 64 bit = 8 bytes)
			/// </summary>
			/// <param name="cmd"></param>
			/// <param name="value"></param>
			/// <param name="varList"></param>
			/// <returns></returns>
			private byte[] ConvertSingleLineOfCodeToByteCode(string cmd, string value, Dictionary<string, string> varList) {
				List<byte> byteCodes = new List<byte>();

				byte cmdByteCode = 0;
				byte valType = TYPE_NULL;
				byte[] byteValue = null;

				// Determine what kind of value type we have for the right side of IASM code
				if(value != "") {
					if(value[0] == '$') {
						// Variable
						string varType = varList[value];
						value = value.Replace("$", "");
						switch(varType) {
							case "u8": valType = TYPE_UVAR8; break;
							case "s8": valType = TYPE_SVAR8; break;
							case "u16": valType = TYPE_UVAR16; break;
							case "s16": valType = TYPE_SVAR16; break;
							case "u32": valType = TYPE_UVAR32; break;
							case "s32": valType = TYPE_SVAR32; break;
							case "u64": valType = TYPE_UVAR64; break;
							case "s64": valType = TYPE_SVAR64; break;
						}
					} else if(value[0] == '#') {
						// Pointer reference
						valType = TYPE_PTR;
						value = value.Replace("#", "");
					} else {
						// Immediate
						valType = TYPE_IMM;
					}
				} else {
					// Set NULL as type
					valType = TYPE_NULL;
				}

				// VARu64, VARs64, etc
				if(cmd.IndexOf(PH_VAR) != -1) {
					cmdByteCode = VAR;
				}

				switch(cmd) {
					case PH_NOP: cmdByteCode = NOP; break;
					case PH_VMC: cmdByteCode = VMC; break;
					case PH_MOV: cmdByteCode = MOV; break;
					case PH_JMP: cmdByteCode = JMP; break;
					case PH_JMS: cmdByteCode = JMS; break;
					case PH_RET: cmdByteCode = RET; break;
					case PH_ADD: cmdByteCode = ADD; break;
					case PH_SUB: cmdByteCode = SUB; break;
					case PH_MLT: cmdByteCode = MLT; break;
					case PH_DIV: cmdByteCode = DIV; break;
					case PH_MOD: cmdByteCode = MOD; break;
					case PH_SHL: cmdByteCode = SHL; break;
					case PH_SHR: cmdByteCode = SHR; break;
					case PH_CAN: cmdByteCode = CAN; break;
					case PH_COR: cmdByteCode = COR; break;
					case PH_SKE: cmdByteCode = SKE; break;
					case PH_AND: cmdByteCode = AND; break;
					case PH_LOR: cmdByteCode = LOR; break;
					case PH_XOR: cmdByteCode = XOR; break;
					case PH_PSH: cmdByteCode = PSH; break;
					case PH_POP: cmdByteCode = POP; break;
					case PH_LEQ: cmdByteCode = LEQ; break;
					case PH_LNE: cmdByteCode = LNE; break;
					case PH_LSE: cmdByteCode = LSE; break;
					case PH_LGE: cmdByteCode = LGE; break;
					case PH_LGS: cmdByteCode = LGS; break;
					case PH_LGG: cmdByteCode = LGG; break;
					case PH_UMN: cmdByteCode = UMN; break;
					case PH_UPL: cmdByteCode = UPL; break;
					case PH_UNT: cmdByteCode = UNT; break;
					case PH_BUC: cmdByteCode = BUC; break;
				}

				byteCodes.Add(cmdByteCode);
				byteCodes.Add(valType);


				if(valType != TYPE_NULL) {
					if(TARGET_ARCH_BIT_SIZE == 64) {
						byteValue = BitConverter.GetBytes(Convert.ToInt64(value));
					} else if(TARGET_ARCH_BIT_SIZE == 32) {
						byteValue = BitConverter.GetBytes(Convert.ToInt32(value));
					} else if(TARGET_ARCH_BIT_SIZE == 16) {
						byteValue = BitConverter.GetBytes(Convert.ToInt16(value));
					}

					if(byteValue != null) {
						for(int i = byteValue.Length - 1; i >= 0; i--) {
							byteCodes.Add(byteValue[i]);
						}
					}
				}

				return byteCodes.ToArray<byte>();
			}

			#endregion

			#region Assembler

			/// <summary>Optimizes memory usage by eliminating redundant temporary variables</summary>
			/// <param name="codes"></param>
			/// <param name="declaredVarList"></param>
			/// <returns>Optimized code</returns>
			public string[] optimizeVariables(string[] codes, Dictionary<string, string> declaredVarList) {
				int numTmpVars = 200;
				// Add global variables that can be used as common variables that can be used as temporary placeholders
				List<string> list = codes.ToList<string>();
				for(int i = 0; i < numTmpVars; i++) {
					list.Insert(0, "long DSCRIPT_TMP_VAR_LONG" + i.ToString());
					list.Insert(1, ";");
					list.Insert(2, "ulong DSCRIPT_TMP_VAR_ULONG" + i.ToString());
					list.Insert(3, ";");
				}

				// For each of the declared var, check if we can replace the temporary variables with the common variables
				int varUnsignedCnt = 0;
				int varSignedCnt = 0;

				for(int i = 0; i < declaredVarList.Count; i++) {
					var item = declaredVarList.ElementAt(i);

					string varToCheck = item.Key;
					// If the variable name contains "RQ20131212DSRNDID", then it means it's an auto generated temporary variable
					if(varToCheck.IndexOf(TmpVarIdentifier) == -1) continue;

					// We'll just do a quick and dirty replacement, where temporary variables are replaced in sequential order,
					// then when we reach the maximum index (numTmpVars), resets and starts from 0 again
					if(item.Value.IndexOf("u", 0) != -1) {
						// Unsigned var
						declaredVarList[item.Key] = "DSCRIPT_TMP_VAR_ULONG" + varUnsignedCnt.ToString();
						varUnsignedCnt++;
						if(varUnsignedCnt >= numTmpVars) varUnsignedCnt = 0;
					} else {
						// Signed var
						declaredVarList[item.Key] = "DSCRIPT_TMP_VAR_LONG" + varSignedCnt.ToString();
						varSignedCnt++;
						if(varSignedCnt >= numTmpVars) varSignedCnt = 0;
					}
				}

				/*
				foreach(var kvp in declaredVarList) {
					string varToCheck = kvp.Key;
					// If the variable name contains "RQ20131212DSRNDID", then it means it's an auto generated temporary variable
					if(varToCheck.IndexOf(TmpVarIdentifier) == -1) continue;

					// We'll just do a quick and dirty replacement, where temporary variables are replaced in sequential order,
					// then when we reach the maximum index (numTmpVars), resets and starts from 0 again
					if(kvp.Value.IndexOf("u", 0) != -1) {
						// Unsigned var
						declaredVarList[kvp.Key] = "DSCRIPT_TMP_VAR_ULONG" + varUnsignedCnt.ToString();
						varUnsignedCnt++;
						if(varUnsignedCnt >= numTmpVars) varUnsignedCnt = 0;
					} else {
						// Signed var
						declaredVarList[kvp.Key] = "DSCRIPT_TMP_VAR_LONG" + varSignedCnt.ToString();
						varSignedCnt++;
						if(varSignedCnt >= numTmpVars) varSignedCnt = 0;
					}
				}
				*/
				// We won't convert "char" variables as we don't know how long it may be
				string[] reservedVariableKeyWords = new string[] {
				PH_RESV_KW_VAR, PH_RESV_KW_OBJECT, PH_RESV_KW_BOOL, PH_RESV_KW_SBYTE, PH_RESV_KW_SHORT, PH_RESV_KW_INT, PH_RESV_KW_LONG, PH_RESV_KW_ULONG,
				PH_RESV_KW_BYTE, PH_RESV_KW_USHORT, PH_RESV_KW_UINT, PH_RESV_KW_FLOAT, PH_RESV_KW_DOUBLE, PH_RESV_KW_DECIMAL,
			};
				string ptrCharIndicator = PH_ID + PH_NEW + PH_ID + PH_RESV_KW_CHAR.Trim();
				// Now, for each code, replace the temporary variables
				for(int i = 0; i < list.Count; i++) {
					if(list[i].IndexOf(TmpVarIdentifier) == -1) continue;
					// Check if we have a variable declaration keyword in front of the temporary var
					string[] chkCode = list[i].Split(' ');
					// If this line of code is a variable declaration, we simply remove it because we have our place holder vars already declared at the top of code
					if(chkCode.Length == 2 && reservedVariableKeyWords.Contains(chkCode[0] + " ")) {
						// However, if the variable contains PH_NEW keyword (pointer) afterwards in the code, then this is possibly a "char" variable
						// We won't convert "char" variables as we don't know how long it may be
						// So, we need to revert back the variable name to its original name
						// i.e. Code structure is like the following if it was a char* variable:
						// -------------------------------------------------------------------
						// long thhbkwiqgkRQ20131212DSRNDID0
						// ;
						// thhbkwiqgkRQ20131212DSRNDID0
						// =
						// 𒀭PH_NEW𒀭char[46]
						// thhbkwiqgkRQ20131212DSRNDID0[0]
						// ....
						if(list[i + 4].IndexOf(ptrCharIndicator, 0) != -1) {
							string originalVarName = list[i + 2];
							if(declaredVarList.ContainsKey(originalVarName)) {
								declaredVarList.Remove(originalVarName);
							}
							continue;
						}

						list[i] = "";
						list[i + 1] = "";
						i++;
						continue;
					}
					string varName = list[i];
					bool isExpressionContainsVar = false;
					bool isRepeatCheckOnExpression = false;
					// A variable name might be contained inside an expression:
					// e.g. "Tableu30A4u30D9u30F3u30C8u30D5u30E9u30B0.Save.startAddress#result_gnggvueqtfRQ20131212DSRNDID#"
					// In this case, we need to extract the variable name from the equation
					if(varName.IndexOf("#") != -1) {
						// The expression contains double quotations on front and back, so we need to remove them first
						varName = varName.Replace("\"", "");
						string[] expression = varName.Split('#');
						for(int j = 0; j < expression.Length; j++) {
							if(declaredVarList.ContainsKey(expression[j]) && expression[j].IndexOf(TmpVarIdentifier) != -1) {
								isExpressionContainsVar = true;
								varName = expression[j];
								isRepeatCheckOnExpression = true;
								break;
							}
						}
					} else if(varName.IndexOf("[") != -1) {
						// We might have arrays with size that is spcified as variable that needs to be replaced
						// i.e. 𒀭PH_NEW𒀭char[apdrlbkfhfRQ20131212DSRNDID]
						string arrayIndexVar = Regex.Match(varName, @"\[(.*?)\]").Groups[1].Value;
						if(arrayIndexVar.IndexOf(TmpVarIdentifier) != -1) {
							varName = arrayIndexVar;
							isExpressionContainsVar = true;
						}
					} else {
						isExpressionContainsVar = declaredVarList.ContainsKey(varName);
					}
					if(!isExpressionContainsVar) continue;
					list[i] = list[i].Replace(varName, declaredVarList[varName]);
					// We might have more than one temporary variables inside expressions, so just repeat the check process again to see if there's any other variables that we need to replace
					if(isRepeatCheckOnExpression) i--;
				}

				// Let's clean the codes again by separating into tokens
				return RegenerateCleanCode(list.ToArray());
			}

			/// <summary>
			/// Converts preprocessed intermediatary source code to Assembly language codes
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			public string[] ConvertIntermCodeToAsm(string[] codes, CompileOptions co) {
				/*
				// If we need to write intermediate code to file, we do so here
				string itermScriptFileName = Path.GetFileNameWithoutExtension(co.FileName) + ".interm";
				string itermScriptOutPutFileName = Path.GetDirectoryName(co.FileName) + Path.DirectorySeparatorChar + itermScriptFileName;
				if(co.IsOutputIntermCode) {
					File.WriteAllLines(itermScriptOutPutFileName, codes);
				}*/

				// Get all of the declared static class constructor list
				List<string> declaredStaticClassConstructorList = GetDeclaredStaticClassConstructorList(codes);
				// Get all of the declared variable list (declared variable name, type), where type = "{u|s}BitSize" (e.g. u64, s32)
				Dictionary<string, string> declaredVarList = GetDeclaredVarList(codes);

				/* CONTAINS ISSUES!!
				ShowLog("- Optimizing temporary variables...");
				codes = optimizeVariables(codes, declaredVarList);
				// Get all of the declared variable list again
				declaredVarList = GetDeclaredVarList(codes);
				*/

				// If we need to write intermediate code to file, we do so here
				string itermScriptFileName = Path.GetFileNameWithoutExtension(co.FileName) + ".interm";
				string itermScriptOutPutFileName = Path.GetDirectoryName(co.FileName) + Path.DirectorySeparatorChar + itermScriptFileName;
				if(co.IsOutputIntermCode) {
					File.WriteAllLines(itermScriptOutPutFileName, codes);
				}

				// Get all the instance object list (instance obj name, class name)
				Dictionary<string, string> instanceObjList = GetInstanceObjects(codes);
				// Get all the function list (function name, type)
				Dictionary<string, string> declaredFuncList = GetDeclaredFunctionList(codes);

				// Remove namespace and scope brackets as we do not need them anymore
				ShowLog("- Removing namespaces...");
				codes = RemoveNamespaces(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_RemoveNamespaces.txt", string.Join("\n", codes));
				// Convert class member variables to pointer declarations/pointer vars
				ShowLog("- Converting class member vars to pointers...");
				codes = PrepareClassStructure(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_PrepareClassStructure.txt", string.Join("\n", codes));
				// Convert all variable declarations to ASM code
				ShowLog("- Converting var declarations to iASM code...");
				codes = ConvertVarDeclarationsToASM(codes, declaredVarList);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertVarDeclarationsToASM.txt", string.Join("\n", codes));
				// Convert increment statements on its own to one line
				ShowLog("- Resolving increment statements...");
				codes = ConvertIncDecToOneLine(codes, "+");
				// Convert decrement statements on its own to one line
				ShowLog("- Resolving decrement statements...");
				codes = ConvertIncDecToOneLine(codes, "-");
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertIncDecToOneLine.txt", string.Join("\n", codes));
				// Resolve stand-alone increment/decrement codes
				ShowLog("- Resolving stand-alone increment/decrement codes...");
				codes = ResolveStandAloneIncrementDecrement(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ResolveStandAloneIncrementDecrement.txt", string.Join("\n", codes));
				// Convert mathmatical expressions to ASM code
				ShowLog("- Converting expressions to iASM code...");
				codes = ConvertExpressionsToASM(codes, declaredVarList, instanceObjList, declaredFuncList);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertExpressionsToASM.txt", string.Join("\n", codes));
				// Converts variables that are class instances to pointer format
				ShowLog("- Converting class instances to pointers...");
				codes = ConvertClassVarDeclarationsToPointers(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertClassVarDeclarationsToPointers.txt", string.Join("\n", codes));
				// Convert return statement to "RET" ASM code
				ShowLog("- Converting return statements to iASM code...");
				codes = ConvertReturnToASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertReturnToASM.txt", string.Join("\n", codes));
				// Converts instance's function calls and direct member variable access codes to ASM code
				ShowLog("- Converting instance function calls and direct member access to iASM code...");
				codes = ConvertInstanceFuncCallAndMemberVarAccessToASM(codes, instanceObjList, declaredVarList);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertInstanceFuncCallAndMemberVarAccessToASM.txt", string.Join("\n", codes));
				// Convert if statements to ASM code
				ShowLog("- Converting if statements to iASM code...");
				codes = ConvertIfStatementToASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertIfStatementToASM.txt", string.Join("\n", codes));
				// Convert function parameters to ASM code
				ShowLog("- Converting function params to iASM code...");
				codes = ConvertFunctionParamsToASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertFunctionParamsToASM.txt", string.Join("\n", codes));
				// Replace other codes to ASM code
				ShowLog("- Replacing keywords to iASM code...");
				codes = ReplaceKeywordsToASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ReplaceKeywordsToASM.txt", string.Join("\n", codes));
				// Convert class/functions to labels
				ShowLog("- Converting class/functions to labels...");
				codes = ConvertClassFunctionsToLabels(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertClassFunctionsToLabels.txt", string.Join("\n", codes));
				// Let's clean the codes again by separating into tokens
				ShowLog("- Regenerating clean code...");
				codes = RegenerateCleanCode(codes);
				// Generate clean ASM code list
				ShowLog("- Generating clean iASM code...");
				codes = GenerateCleanASMCode(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_GenerateCleanASMCode.txt", string.Join("\n", codes));
				// Convert pointer identifier (PH_ID + PH_POINTER + PH_ID) to "*" symbol
				ShowLog("- Replacing pointer identifiers...");
				codes = ConvertPointerKeywordsToSymbol(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertPointerKeywordsToSymbol.txt", string.Join("\n", codes));
				// Adds VM specific codes (like preserved variables)
				ShowLog("- Adding VM specific codes (preserved vars)...");
				codes = AddVMSpecificCode(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_AddVMSpecificCode.txt", string.Join("\n", codes));
				// Optimizes ASM code to remove redundant codes
				ShowLog("- Removing redundant iASM codes...");
				codes = OptimizeRedundantASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_OptimizeRedundantASM.txt", string.Join("\n", codes));

				/*
				for(int i = 0; i < codes.Length; i++) {
					// If a # symbol is found, it is a pointer, so we need to update the variable use type to VARsXX
					int indexOfSharp = codes[i].IndexOf(" #");
					if(indexOfSharp != -1) {
						if(codes[i].IndexOf(PH_VAR, 0, PH_VAR.Length) == -1 && codes[i].IndexOf(PH_REG, 0, PH_REG.Length) == -1) continue;
						if(codes[i].IndexOf(TARGET_ARCH_BIT_SIZE.ToString(), 4, 2) == -1) {
							codes[i] = PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + codes[i].Substring(indexOfSharp);
						}
					}
				}*/

				List<string> allREGKeywords = GenerateRegVarKeywords(PH_REG);
				List<string> allVarKeywords = GenerateRegVarKeywords(PH_VAR);
				List<string> registeredVarList = GetRegisteredVarList(codes, allREGKeywords);

				// Move all REG operations to top of code, and create a "main" label for the rest of code
				ShowLog("- Moving all REG operations to top of code/generating main label...");
				codes = MoveREGVarAndVarInitiationToTopOfCode(codes, allREGKeywords, registeredVarList, declaredStaticClassConstructorList);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_MoveREGVarAndVarInitiationToTopOfCode.txt", string.Join("\n", codes));
				// Convert REG{u|v}XXX will be converted to VAR{u|v}XXX ---> The VM will register the first appearing variables as new variables automatically,
				// Hence we won't need to distinguish between REG and VAR
				ShowLog("- Converting REG keywords to VAR keyword...");
				codes = ConvertRegToVarCommand(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertRegToVarCommand.txt", string.Join("\n", codes));

				// For exporting iasm simulator debugging code: Remove all labels and convert label references on JMP and JMS to absolute program code index
				ShowLog("- Removing labels and converting JMP/JMS instruction to jump to code index...");
				string[] iasmCodes = new string[codes.Length];
				for(int i = 0; i < codes.Length; i++) iasmCodes[i] = codes[i];
				iasmCodes = RemoveLabelsAndConvertJumpToCodeIndex(iasmCodes);

				// If we need to write intermediate code to file, we do so here
				string filePath = Path.GetDirectoryName(co.FileName) + Path.DirectorySeparatorChar;
				string asmFileName = Path.GetFileNameWithoutExtension(co.FileName) + ".iasm";
				string iasmOutPutFileName = filePath + asmFileName;
				if(co.IsOutputIntermCode) {
					ShowLog("- Exporting itermediate code...");
					File.WriteAllLines(iasmOutPutFileName, iasmCodes);
				}

				// Remove any blank array list items
				ShowLog("- Removing blank items...");
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();

				// Add extra line to the code for the @END label
				List<string> codeList = codes.ToList<string>();
				codeList.Add("");
				codes = codeList.ToArray<string>();

				// Convert all variable names that are declared with REG and VAR to numeric indexes with preceeding "$" symbol
				ShowLog("- Converting variables to IDs...");
				codes = ConvertVariablesToID(codes, registeredVarList);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertVariablesToID.txt", string.Join("\n", codes));

				// Convert all labels to numeric IDs
				ShowLog("- Converting all labels to numeric IDs...");
				codes = ConvertJumpAndLabelsToNumericID(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestConvertIntermCodeToAsm_ConvertJumpAndLabelsToNumericID.txt", string.Join("\n", codes));

				return codes;
			}

			/// <summary>
			/// Convert all labels to numeric IDs, and the labels referenced by JMP and JMS to the equivalent ID
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertJumpAndLabelsToNumericID(string[] codes) {
				// Get the list of labels (label name, label ID in numeric form)
				Dictionary<string, int> labelList = new Dictionary<string, int>();
				int labelID = 0;
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode[0] != PH_NOP) continue;
					labelList.Add(splitCode[1], labelID);
					codes[i] = PH_NOP + " " + labelID++;
				}
				// For each code, replace JMP/JMS labels to numeric label IDs
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode[0] != PH_JMP && splitCode[0] != PH_JMS) continue;
					codes[i] = splitCode[0] + " " + labelList[splitCode[1]];
				}

				return codes;
			}

			/// <summary>
			/// Deploy all manually coded IASM codes in the script
			/// Typically, after several pre-processing, the IASM code will have the following structure by now:
			/// __________________________________
			/// int yyy
			/// ;
			/// long xxx
			/// ;
			/// xxx
			/// =
			/// 𒀭PH_STR0𒀭
			/// ;
			/// yyy
			/// =
			/// DSystem.IASM.__iasm
			/// (
			/// xxx
			/// )
			/// ;
			/// __________________________________
			///
			/// Therefore, we will remove the entire code above and replace it with the actual value of 𒀭PH_STRø𒀭
			/// (Where ø is the string list ID)
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] DeployManualIASM(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != PH_RESV_KW_IASM_FUNCTION_CALL) continue;
					// If "DScript.VMC.__iasm" was found, find the code index for ";" which is found after the function call,
					// and also find the code index where the string "VARs64 REG_SVAR0" is found
					// By the way, "VARs64 REG_SVAR0" is inserted automatically to avoid floating function calls - but we won't need this anymore
					int startIndex = 0;
					int endIndex = 0;
					// Get the parameter parsed to the IASM call (which is the placeholder variable that stores the IASM code string
					// int yyy = DSystem.IASM.__iasm(xxx); <-------- "yyy" is the variable that stores the dummy return value
					string dummyPopVariableOfCall = codes[i - 2];
					for(int j = i; j < codes.Length; j++) {
						if(codes[j] == ";") {
							endIndex = j;
							break;
						}
					}
					string startCodeToSearch = PH_RESV_KW_INT + dummyPopVariableOfCall;
					string placeHolderString = "";
					for(int j = i; j >= 0; j--) {
						// If we find the placeholder string that stores the IASM code as we trace back the code, store it along with the variable name that stores this string
						if(codes[j].IndexOf(PH_ID + PH_STR) != -1) {
							placeHolderString = codes[j];
							continue;
						}
						if(codes[j] == startCodeToSearch) {
							startIndex = j;
							break;
						}
					}
					// Remove all code from the start to end index
					for(int j = startIndex; j <= endIndex; j++) {
						codes[j] = "";
					}
					// Add the IASM code to the first code index
					codes[startIndex] = StringLiteralList[placeHolderString].Replace("\"", "") + ";";
				}
				return codes;
			}

			/// <summary>
			/// Replace other codes to ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ReplaceKeywordsToASM(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length < 1) continue;
					// Convert labels to "NOP LABEL_NAME" format
					if(splitCode[0].IndexOf(PH_ID + PH_LABEL + PH_ID) != -1) {
						codes[i] = PH_NOP + " " + codes[i].Replace(PH_ID + PH_LABEL + PH_ID, "");
						continue;
					}
					// Remove any occurance of 𒀭PH_LBL𒀭
					codes[i] = codes[i].Replace(PH_ID + PH_LABEL + PH_ID, "");
					// Convert goto to JMP commands
					if(splitCode[0] + " " == PH_RESV_KW_GOTO) {
						codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_GOTO, PH_JMP + " ");
					}
				}
				return codes;
			}

			/// <summary>
			/// Remove all labels and convert label references on JMP and JMS to absolute program code index
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] RemoveLabelsAndConvertJumpToCodeIndex(string[] codes) {
				// Get the list of labels (label name, code index)
				Dictionary<string, string> labelList = new Dictionary<string, string>();
				int reducedLineOfCode = 0;
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode[0] != PH_NOP) continue;
					int codeIndex = i - (reducedLineOfCode++);
					labelList.Add(splitCode[1], codeIndex.ToString());
					codes[i] = "";
				}
				// For each code, replace JMP/JMS labels to actual code index
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode[0] != PH_JMP && splitCode[0] != PH_JMS) continue;
					codes[i] = splitCode[0] + " " + labelList[splitCode[1]];
				}
				// Remove any blank array list items
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();

				// Add extra line to the code for the @END label
				List<string> codeList = codes.ToList<string>();
				codeList.Add("");
				codes = codeList.ToArray<string>();

				return codes;
			}

			/// <summary>
			/// Adds VM specific codes (like preserved variables)
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] AddVMSpecificCode(string[] codes) {
				List<string> newCodes = codes.ToList<string>();
				// Pre-defined variable declarations
				newCodes.Insert(0, PH_REG + "s" + TARGET_ARCH_BIT_SIZE + " " + REG_SVAR1);
				newCodes.Insert(0, PH_REG + "s" + TARGET_ARCH_BIT_SIZE + " " + REG_SVAR0);
				newCodes.Insert(0, PH_REG + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR1);
				newCodes.Insert(0, PH_REG + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0);
				newCodes.Insert(0, PH_REG + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_NULL);
				codes = newCodes.ToArray<string>();
				return codes;
			}

			/// <summary>
			/// Converts REG to VAR
			/// REG{u|v}XXX will be converted to VAR{u|v}XXX ---> The VM will register the first appearing variables as new variables automatically,
			/// Hence we won't need to distinguish between REG and VAR
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertRegToVarCommand(string[] codes) {
				// Store all the registered variables with (varName, ID) pairs, and convert variables declared with REG{u|s}XXX with the ID
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					codes[i] = splitCode[0].Replace(PH_REG, PH_VAR) + " " + splitCode[1];
				}
				return codes;
			}

			/// <summary>
			/// Convert all variable names that are declared with REG and VAR to numeric indexes with preceeding "$" symbol
			/// If a variable is declared as pointer, or referencing a pointer, it is added with a preceeding "#" symbol instead
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="registeredVarList"></param>
			/// <returns></returns>
			private string[] ConvertVariablesToID(string[] codes, List<string> registeredVarList) {
				Dictionary<string, string> regVarIndexList = new Dictionary<string, string>();
				// Store all the registered variables with (varName, ID) pairs, and convert variables declared with REG{u|s}XXX with the ID
				// NOTE: Remember that the first 3 lines of code is a JMS code that jumps to DScript_Main label, so we need to ignore them
				for(int i = 3; i < registeredVarList.Count + 3; i++) {
					int varID = i - 3;
					string[] splitVar = registeredVarList[varID].Split(' ');
					string newVarType = splitVar[0].Replace(PH_REG, PH_VAR);
					//string varAddressRefSymbol = (splitVar[1].IndexOf("*") == -1) ? "$" : "*";
					string varAddressRefSymbol = "$";
					regVarIndexList.Add(splitVar[1].Replace("#", ""), varAddressRefSymbol + varID.ToString());
					string[] splitCode = codes[i].Split(' ');
					codes[i] = newVarType + " " + varAddressRefSymbol + varID.ToString();
				}
				// For all variables in the code, convert to IDs
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					string varToCheck = splitCode[1].Replace("#", "");
					if(!regVarIndexList.ContainsKey(varToCheck)) continue;
					codes[i] = codes[i].Replace(varToCheck, regVarIndexList[varToCheck]);
					if(splitCode[1].IndexOf("#") != -1) codes[i] = codes[i].Replace("$", "");
				}
				// Now, for the registered static pointer variables (variables declared with #), replace $ with #
				// NOTE: Remember that the first 3 lines of code is a JMS code that jumps to DScript_Main label, so we need to ignore them
				for(int i = 3; i < registeredVarList.Count + 3; i++) {
					int varID = i - 3;
					if(registeredVarList[varID].IndexOf("#") == -1) continue;
					codes[i] = codes[i].Replace("$", "#");
				}
				return codes;
			}

			/// <summary>
			/// Returns the list of variables that are registered with the REG keyword
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="allREGKeywords">List of possible REG keywords</param>
			/// <returns></returns>
			private List<string> GetRegisteredVarList(string[] codes, List<string> allREGKeywords) {
				List<string> registeredVarList = new List<string>();
				// Get all of the REG{u|s}XXX command line and remove them from code
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!allREGKeywords.Contains<string>(splitCode[0])) continue;
					// If REG keyword exist, store in our registerVarList and remove this line of code
					registeredVarList.Add(codes[i]);
				}
				return registeredVarList;
			}

			/// <summary>
			/// Generate all possible REG{u|s}XXX keywords
			/// </summary>
			/// <param name="keyword"></param>
			/// <returns></returns>
			private List<string> GenerateRegVarKeywords(string keyword) {
				List<string> allRegVarKeywords = new List<string>();
				for(int i = 8; i <= TARGET_ARCH_BIT_SIZE; i *= 2) {
					allRegVarKeywords.Add(keyword + "u" + i);
					allRegVarKeywords.Add(keyword + "s" + i);
				}
				return allRegVarKeywords;
			}

			/// <summary>
			/// Move all REG operations to top of code, followed by initiation code for calls to static class constructors
			/// Then, the code will be followed by a "main" label we create which will be the starting point of program
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="allREGKeywords"></param>
			/// <param name="registeredVarList"></param>
			/// <param name="declaredStaticClassConstructorList"></param>
			/// <returns></returns>
			private string[] MoveREGVarAndVarInitiationToTopOfCode(string[] codes, List<string> allREGKeywords, List<string> registeredVarList, List<string> declaredStaticClassConstructorList) {
				List<string> newCode = new List<string>();
				// Get all of the REG{u|s}XXX command line and remove them from code
				for(int i = 0; i < codes.Length; i++) {
					if(!registeredVarList.Contains<string>(codes[i])) continue;
					codes[i] = "";
				}
				foreach(string s in registeredVarList) newCode.Add(s);

				// Generate calls to static class constructors
				foreach(string s in declaredStaticClassConstructorList) {
					newCode.Add(PH_JMS + " " + s);
					newCode.Add(PH_POP + " " + REG_SVAR0);
				}

				// Add float precision define code here
				newCode.Add(PH_PSH + " " + DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY);
				newCode.Add(PH_JMS + " " + PH_RESV_SYSFUNC_SET_FLOAT_PRECISION);
				// REMEMBER: Any JMS calls that do not return any value mush have "POP REG_UVAR0" to pop unwanted return 0 value in stack!!
				newCode.Add(PH_POP + " " + REG_SVAR0);

				// Add a JMS command to jump to the init label
				newCode.Add(PH_JMS + " " + "@DScript_Init");
				// REMEMBER: Any JMS calls that do not return any value mush have "POP REG_UVAR0" to pop unwanted return 0 value in stack!!
				newCode.Add(PH_POP + " " + REG_SVAR0);
				// Add a JMP @END command that indicates the program to complete running and to terminate
				newCode.Add(PH_JMP + " " + "@END");

				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == "") continue;
					newCode.Add(codes[i]);
				}
				// Add "end" label to the bottom of our code to indicate the end of code
				newCode.Add(PH_NOP + " " + "@END");

				// Insert JMS command at the very top of code to jump the main program entry point
				// NOTE: This first set of 3 line codes will be ignored by the VM at first run
				newCode.Insert(0, PH_JMP + " " + "@END");
				newCode.Insert(0, PH_POP + " " + REG_SVAR0);
				newCode.Insert(0, PH_JMS + " " + "@DScript_Main");

				codes = newCode.ToArray<string>();
				return codes;
			}

			/// <summary>
			/// Optimizes ASM code to remove redundant codes
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] OptimizeRedundantASM(string[] codes) {
				// Remove redundant code that pushes and pops same variable: "PSH REG_SVAR0 POP REG_SVAR0"
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(splitCode[0] != PH_PSH) continue;
					string[] splitNextCode = codes[i + 1].Split(' ');
					if(splitNextCode.Length == 1) continue;
					if(splitNextCode[0] != PH_POP) continue;
					if(splitCode[1] != splitNextCode[1]) continue;
					// If we have a code that push & pop the same variable, remove these codes
					codes[i] = "";
					codes[i + 1] = "";
				}
				// Remove any blank array list items
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();
				return codes;
			}

			/// <summary>
			/// Generate clean ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] GenerateCleanASMCode(string[] codes) {
				string mergedCode = "";
				for(int i = 0; i < codes.Length; i++) mergedCode += codes[i] + " ";
				mergedCode = Cleanup(mergedCode);
				mergedCode = mergedCode.Replace(";", " ");
				string[] splitCode = mergedCode.Split(' ');
				string newCode = "";
				for(int i = 0; i < splitCode.Length; i++) {
					if(splitCode[i] == PH_RET || splitCode[i] == PH_UMN || splitCode[i] == PH_UPL || splitCode[i] == PH_UNT || splitCode[i] == PH_BUC) {
						newCode += splitCode[i] + ";";
						continue;
					}
					newCode += splitCode[i] + " " + splitCode[i + 1] + ";";
					i++;
				}
				// Split codes again
				codes = newCode.Split(';');
				// Remove any blank array list items
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();

				return codes;
			}

			/// <summary>
			/// Convert pointer identifier (PH_ID + PH_POINTER + PH_ID) to "#" symbol
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertPointerKeywordsToSymbol(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					codes[i] = codes[i].Replace(PH_ID + PH_POINTER + PH_ID, "#");
				}
				return codes;
			}

			/// <summary>
			/// Clean the codes by separating into tokens
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] RegenerateCleanCode(string[] codes) {
				string newCodes = "";
				for(int i = 0; i < codes.Length; i++) newCodes += codes[i];
				codes = splitCodesToArray(newCodes);
				return codes;
			}

			/// <summary>
			/// Convert function parameters to POP ASM codes
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertFunctionParamsToASM(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(!AllFunctionKeywords.Contains<string>(splitCode[0] + " ")) continue;
					// Get the codes inside round brackets
					int startRoundBracketIndex = i + 1;
					int endRoundBracketIndex = FindEndScopeBlock(codes, startRoundBracketIndex, "(", ")");
					// Get contents inside the round brackets
					string funcParams = "";
					for(int j = startRoundBracketIndex + 1; j < endRoundBracketIndex; j++) {
						funcParams += codes[j];
					}
					// Remove all contents inside and including the round brackets
					for(int j = startRoundBracketIndex; j < endRoundBracketIndex + 1; j++) {
						codes[j] = "";
					}
					if(funcParams != "") {
						string[] splitParams = funcParams.Split(',');
						funcParams = "";
						// We need to reverse parameter order when poping, because our stack is First-In-Last-Out
						for(int j = splitParams.Length - 1; j >= 0; j--) {
							string[] spltParamVar = splitParams[j].Split(' ');
							string varType = spltParamVar[0].Replace(PH_REG, PH_VAR);
							string varName = spltParamVar[1];
							funcParams += splitParams[j] + " " + PH_POP + " " + varName + ";";
						}
					}
					codes[endRoundBracketIndex + 2] = funcParams + codes[endRoundBracketIndex + 2];
				}
				return codes;
			}

			/// <summary>
			/// Converts classes and functions to labels
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertClassFunctionsToLabels(string[] codes) {
				// Convert class to labels
				codes = ConvertKeywordsToLabels(codes, PH_RESV_KW_STATIC_CLASS);
				codes = ConvertKeywordsToLabels(codes, PH_RESV_KW_CLASS);
				// Convert functions to labels
				for(int i = 0; i < AllFunctionKeywords.Length; i++) {
					codes = ConvertKeywordsToLabels(codes, AllFunctionKeywords[i]);
				}
				return codes;
			}

			/// <summary>
			/// Converts given keyword (i.e. "class ", "static_class ", "function_void ", etc) to labels
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="keyword"></param>
			/// <returns></returns>
			private string[] ConvertKeywordsToLabels(string[] codes, string keyword) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(keyword, 0) != -1) {
						int scopeBracketStartIndex = i + 1;
						for(int j = i; j < codes.Length; j++) {
							if(codes[j] == "{") {
								scopeBracketStartIndex = j;
								break;
							}
						}
						string labelName = ReplaceFirstOccurance(codes[i], keyword, "");
						string derivedClassName = labelName;
						// If dealing with class, we check if it has a base class indication and remove if so
						if((keyword == PH_RESV_KW_CLASS) && (codes[i + 1] == ":")) {
							labelName += "_" + codes[i + 2];
							codes[i + 1] = PH_NOP + " " + derivedClassName + ";";
							codes[i + 2] = "";
							scopeBracketStartIndex = i + 3;
						}
						codes[i] = PH_NOP + " " + labelName;
						codes[i] += ";";
						int scopeBracketEndIndex = FindEndScopeBlock(codes, scopeBracketStartIndex, "{", "}");
						codes[scopeBracketStartIndex] = "";
						codes[scopeBracketEndIndex] = PH_NOP + " " + labelName + "_end;";
					}
				}
				return codes;
			}

			/// <summary>
			/// Convert increment/decrement statements on its own to one line
			/// (e.g.)
			/// [0] i  [1] +  [2] +  ----> [0] i++
			/// [0] -  [1] -  [2] i  ----> [0] --i
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="operatorSingleSymbol">"+" or "-"</param>
			/// <returns></returns>
			private string[] ConvertIncDecToOneLine(string[] codes, string operatorSingleSymbol) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != operatorSingleSymbol) continue;
					if(codes[i + 1] != operatorSingleSymbol) continue;
					codes[i] = "";
					codes[i + 1] = "";
					// Determine if this is a pre-increment or post-increment
					if(codes[i + 2] == ";") {
						// Post-increment
						codes[i - 1] += operatorSingleSymbol + operatorSingleSymbol;
					} else {
						// Pre-increment
						codes[i + 2] = operatorSingleSymbol + operatorSingleSymbol + codes[i + 2];
					}

				}

				return codes;
			}

			/// <summary>
			/// Converts if else if else statements to ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertIfStatementToASM(string[] codes) {
				string ifBlockScopeEndLabel = "";
				for(int i = 0; i < codes.Length; i++) {
					// We don't need else statements to be processed
					if(codes[i] == PH_RESV_KW_ELSE) {
						int endOfElseBlock = FindEndScopeBlock(codes, i, "{", "}");
						codes[i] = "";
						codes[i + 1] = "";
						codes[endOfElseBlock] = "";
						continue;
					}
					if(codes[i] != PH_RESV_KW_IF && codes[i] != PH_RESV_KW_ELSEIF) continue;
					if(codes[i] == PH_RESV_KW_IF) ifBlockScopeEndLabel = "";
					// if statement are already converted to have only one variable for evaluation
					// (e.g.) if(xxx) { ... }
					// Therefore, we will just get the variable for evaluation and remove the evaluation statement
					string varToEvaluate = codes[i + 2];
					string ifScopeEndLabel = codes[i - 2].Replace("_begin_", "_end_");
					if(ifBlockScopeEndLabel == "") ifBlockScopeEndLabel = codes[i - 4].Replace("_begin_", "_end_");
					for(int j = 0; j < 4; j++) codes[i + j] = "";
					int startScopeIndex = i + 4;
					int endScopeIndex = FindEndScopeBlock(codes, startScopeIndex, "{", "}");
					string expression = PH_VAR + "u" + BYTE_BIT_SIZE + " " + varToEvaluate + " " + PH_SKE + " 1 " + PH_JMP + " " + ifScopeEndLabel + " ";
					// Remove scope brackets
					codes[startScopeIndex] = "";
					codes[endScopeIndex] = PH_JMP + " " + ifBlockScopeEndLabel + " ";
					// Insert generated ASM code
					codes[i] = expression;
					//i = endScopeIndex;
				}

				return codes;
			}

			/// <summary>
			/// Converts instance's function calls and direct member variable access codes to ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="instanceObjList"></param>
			/// <param name="declaredVarList"></param>
			/// <returns></returns>
			private string[] ConvertInstanceFuncCallAndMemberVarAccessToASM(string[] codes, Dictionary<string, string> instanceObjList, Dictionary<string, string> declaredVarList) {
				// Get all function list and store in a dictionary with: (function name, return type)
				Dictionary<string, string> funcList = GetAllFunctionList(codes);
				// For each instance object list, add a period
				Dictionary<string, string> instanceObjMemberAccessibleList = new Dictionary<string, string>();
				foreach(var entry in instanceObjList) instanceObjMemberAccessibleList.Add(entry.Key + ".", entry.Value);

				// For each line of code, search for the instance object that contains a member variable access or function call
				// (i.e. N.Program.Main.a.)  -->  Note the period added to the instance object. This denotes that this object is going to call/access a member
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
					for(int j = 0; j < splitCode.Length; j++) {
						string[] objPeriodSplit = splitCode[j].Split('.');
						if(objPeriodSplit.Length == 1) continue;
						string objInstToCheck = "";
						for(int k = 0; k < objPeriodSplit.Length - 1; k++) {
							// Remove pointers for now
							objPeriodSplit[k] = objPeriodSplit[k].Replace(PH_ID + PH_POINTER + PH_ID, "");
							objInstToCheck += objPeriodSplit[k] + ".";
						}
						if(!instanceObjMemberAccessibleList.ContainsKey(objInstToCheck)) continue;
						// Remove pointers for now
						splitCode[j] = splitCode[j].Replace(PH_ID + PH_POINTER + PH_ID, "");
						// Instance object with member accessing found
						string objInstName = objInstToCheck.Substring(0, objInstToCheck.Length - 1);
						// Now check if this access is a function call or a member variable access
						string memberValue = ReplaceFirstOccurance(splitCode[j], objInstToCheck, instanceObjList[objInstName] + ".");
						//memberValue = Regex.Replace(memberValue, @"\((.*?)\)", ""); // ADDED 2022/03/25
						// If we have a floating dot after the memberValue, it's calling a constructor (with parameters)
						/*if(memberValue.Substring(memberValue.Length - 1, 1) == ".") {
							string[] objSep = memberValue.Split('.');
							memberValue = memberValue + objSep[objSep.Length - 2] + "_Constructor";
						}*/
						// If a member function access, replace this object instance with "PSH [N.Program.Main.a] JMS N.INT32 POP [N.Program.Main.a] N.INT32.get"
						if(funcList.ContainsKey(memberValue)) {
							// It's a function call
							// e.g.
							// "JMS N.Program.Main.a.get POP REG_VAR0 VARs32 N.Program.Main.b MOV   REG_VAR0"
							// Instance FUNCTION CALL:
							// Convert "N.Program.Main.a." to "N.INT32.", and add "PSH N.Program.Main.a JMS N.INT32 POP N.Program.Main.a"
							// ----> [PSH N.Program.Main.a JMS N.INT32 POP N.Program.Main.a N.INT32.get] POP REG_VAR0 VARs32 N.Program.Main.b MOV   REG_VAR0
							splitCode[j] = PH_PSH + " " + objInstName + " " + PH_JMS + " " + instanceObjList[objInstName] + " " +
								PH_POP + " " + objInstName + " " + PH_JMS + " " + memberValue;
							// If it was a function call, we will always have "JMS" in front of this newely generated code, but we will remove it as we don't need it
							splitCode[j - 1] = "";
						} else {
							// If this is not a function call, then it's a member variable access
							// Instance MEMBER ACCESS:
							// e.g. 
							// "VARs32 N.Program.Main.a.value MOV 10"
							// "PSH N.Program.Main.a.value POP REG_VAR1"
							// ---------------------------------------
							// Convert "N.Program.Main.a." to "PSH N.INT32.", and add to front: "JMS N.INT32 POP N.Program.Main.a"
							// ----> [PSH N.Program.Main.a JMS N.INT32 POP N.Program.Main.a VARs32 *N.INT32.value] MOV 10
							string bitSize = declaredVarList[memberValue];
							// ASM code just before this variable is always the operation code towards the target variable
							// So, we will insert the address assignemnt code first
							splitCode[j - 1] = PH_PSH + " " + objInstName + " " + PH_JMS + " " + instanceObjList[objInstName] + " " + PH_POP + " " + objInstName + " " + splitCode[j - 1];
							// Then, we convert the variable to actual pointer variable representation by replacing it with the actual class name
							splitCode[j] = PH_ID + PH_POINTER + PH_ID + memberValue;
						}
						string newCode = "";
						for(int k = 0; k < splitCode.Length; k++) {
							if(splitCode[k] != "") newCode += splitCode[k] + " ";
						}
						codes[i] = newCode;
					}
				}
				return codes;
			}

			/// <summary>
			/// Convert return statement to "RET" ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertReturnToASM(string[] codes) {
				// For each line of code, search for "return"
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] + " " != PH_RESV_KW_RETURN) continue;
					// Found "return", so get the one and only value to be returned (note we have already pre-processed any expression to be returned into one value/variable)
					// The "return" code should look something like this:
					// [0] = return
					// [1] = (
					// [2] = 0
					// [3] = )
					// [4] = ;
					codes[i] = "";
					codes[i + 1] = "";
					codes[i + 2] = PH_PSH + " " + codes[i + 2] + " " + PH_RET;
					codes[i + 3] = "";
					i += 4;
				}
				return codes;
			}

			/// <summary>
			/// Returns all the instance object found in the code, as <fullVarName, className>
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			private Dictionary<string, string> GetInstanceObjects(string[] codes) {
				Dictionary<string, string> instanceObjList = new Dictionary<string, string>();
				// Get all pre-defined keywords for var declarations
				List<string> mergedList = ClassMemberReservedKeyWords.ToList<string>();
				// Get all classes
				List<string> userClasses = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false).ToList<string>();
				mergedList.AddRange(userClasses);
				string[] classDeclarationList = mergedList.ToArray<string>();
				for(int i = 0; i < classDeclarationList.Length; i++) classDeclarationList[i] = classDeclarationList[i].Trim();

				// Get all the instance object list (search for "𒀭PH_NEW𒀭{CLASSNAME}" -> e.g. "𒀭PH_NEW𒀭N.INT32")
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(PH_ID + PH_NEW + PH_ID) == -1) continue;
					string varName = codes[i - 2];
					// Get the class of this variable
					string className = codes[i].Replace(PH_ID + PH_NEW + PH_ID, "");
					// Remove anything after opening square brackets to get the correct class name
					className = Regex.Replace(className, @"\[(.*?)$", "");
					if(!classDeclarationList.Contains<string>(className)) {
						continue;
					}
					// We might have a variable type in front of the varName, so in that case we remove it
					string[] splitCheck = varName.Split(' ');
					if(splitCheck.Length > 1) {
						varName = splitCheck[1];
					}
					// Remove anything after opening square brackets to get the correct var name
					varName = Regex.Replace(varName, @"\[(.*?)$", "");
					if(!instanceObjList.ContainsKey(varName)) instanceObjList.Add(varName, className);
				}

				// Get all user-created class names, and its instance variable list
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length < 2) continue;
					if(!userClasses.Contains(splitCode[0])) continue;
					if(!instanceObjList.ContainsKey(splitCode[1])) instanceObjList.Add(splitCode[1], splitCode[0]);
				}

				// Get all pointer variables that are not instantiated, but we need to treat them as one of the instantiated objects because instantiated references may be passed to it
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode[0] + " " == PH_RESV_KW_PTR) {
						if(!instanceObjList.ContainsKey(splitCode[1])) instanceObjList.Add(splitCode[1], splitCode[0]);
					}
				}

				return instanceObjList;
			}

			/// <summary>
			/// Get all function parameter list within the code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private List<string> GetAllFunctionParameterList(string[] codes) {
				// Get all function parameter list within the code
				List<string> declaredFuncParamList = new List<string>();
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(!AllFunctionKeywords.Contains<string>(splitCode[0] + " ")) continue;
					int roundBracketStartIndex = i + 1;
					int roundBracketEndIndex = FindEndScopeBlock(codes, roundBracketStartIndex, "(", ")");
					string parameters = "";
					for(int j = roundBracketStartIndex + 1; j <= roundBracketEndIndex - 1; j++) {
						parameters += codes[j];
					}
					if(parameters == "") continue;
					string[] splitParams = parameters.Split(',');
					for(int j = 0; j < splitParams.Length; j++) {
						// Currently, the parameter contains the value type + variable name (i.e. "int myvar"), so we ommit the type
						string[] splitParamItem = splitParams[j].Split(' ');
						declaredFuncParamList.Add(splitParamItem[1]);
					}
				}
				return declaredFuncParamList;
			}

			/// <summary>
			/// Convert all variable declarations to ASM code
			/// (e.g.)
			/// int N.Program.Main.a
			/// --->
			/// REGs32 N.Program.Main.a
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="declaredVarList"></param>
			/// <returns></returns>
			private string[] ConvertVarDeclarationsToASM(string[] codes, Dictionary<string, string> declaredVarList) {
				// Get all function parameter list within the code
				List<string> declaredFuncParamList = GetAllFunctionParameterList(codes);

				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!ClassMemberReservedKeyWords.Contains<string>(splitCode[0] + " ")) continue;
					string varTypeBit = "";
					try { varTypeBit = declaredVarList[splitCode[1]]; } catch { continue; }

					// Check to see if we have an array pointer
					string pointerIdentifier = "";
					if(splitCode[0] + " " == PH_RESV_KW_PTR) {
						// If this variable is inside a function parameter, we don't want to convert it to a pointer
						// (as anything parsed into this parameter will be the address of a newly allocated address)
						if(!declaredFuncParamList.Contains<string>(splitCode[1])) {
							pointerIdentifier = PH_ID + PH_POINTER + PH_ID;
						}
					}

					// Found a varivarTypeBitable declaration, so we convert this to ASM code ("REGXX varname" - where XX is the bit size)
					codes[i] = ReplaceFirstOccurance(codes[i], splitCode[0] + " ", ConvertVarTypeToASMPrefix(splitCode[0] + " ", PH_REG) + " " + pointerIdentifier);
				}
				return codes;
			}

			/// <summary>
			/// Converts variables that are class instances to pointer format
			/// (e.g.)
			/// N.INT32 N.Program.Main.a
			/// --->
			/// REGu64 𒀭PH_PTR𒀭N.Program.Main.a
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertClassVarDeclarationsToPointers(string[] codes) {
				// Get all classes
				string[] classDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);
				// Get all function parameter list within the code
				List<string> declaredFuncParamList = GetAllFunctionParameterList(codes);
				// Search the entire code to see if we have any declaration of variables for any of these classes
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!classDeclarationList.Contains<string>(splitCode[0])) continue;
					string pointerSymbol = PH_ID + PH_POINTER + PH_ID;
					// If this variable is inside a function parameter, we don't want to convert it to a pointer
					// (as anything parsed into this parameter will be the address of a newly allocated address)
					if(declaredFuncParamList.Contains<string>(splitCode[1])) {
						pointerSymbol = "";
					}
					// Found a variable of a class type declaration, so we make this a pointer
					codes[i] = ReplaceFirstOccurance(codes[i], splitCode[0] + " ", PH_REG + "s" + TARGET_ARCH_BIT_SIZE + " " + pointerSymbol);
				}
				return codes;
			}

			/*
			 * 			// Get all function parameter list within the code
				List<string> declaredFuncParamList = GetAllFunctionParameterList(codes);

				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!ClassMemberReservedKeyWords.Contains<string>(splitCode[0] + " ")) continue;
					string varTypeBit = "";
					try { varTypeBit = declaredVarList[splitCode[1]]; } catch { continue; }

					// Check to see if we have an array pointer
					string pointerIdentifier = "";
					if(splitCode[0] + " " == PH_RESV_KW_PTR) {
						// If this variable is inside a function parameter, we don't want to convert it to a pointer
						// (as anything parsed into this parameter will be the address of a newly allocated address)
						if(!declaredFuncParamList.Contains<string>(splitCode[1])) {
							pointerIdentifier = PH_ID + PH_POINTER + PH_ID;
						}
					}
			 * */

			/// <summary>
			/// Gets the entire function list that are declared in the code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private Dictionary<string, string> GetDeclaredFunctionList(string[] codes) {
				// Get all variable declarations and store in a Dictionary List (key = variable name, value = bit size)
				Dictionary<string, string> declaredFuncList = new Dictionary<string, string>();
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!AllFunctionKeywords.Contains<string>(splitCode[0] + " ")) continue;
					declaredFuncList.Add(splitCode[1], splitCode[0]);
				}
				return declaredFuncList;
			}

			/// <summary>
			/// Gets the entire static classe's constructor list that are declared in the code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private List<string> GetDeclaredStaticClassConstructorList(string[] codes) {
				// Get all variable declarations and store in a Dictionary List (key = variable name, value = bit size)
				List<string> declaredStaticClassList = new List<string>();
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length != 2) continue;
					if(splitCode[0] + " " != PH_RESV_KW_STATIC_CLASS) continue;
					string[] splitClassName = splitCode[1].Split('.');
					string classNameWithoutScope = splitClassName[splitClassName.Length - 1];
					declaredStaticClassList.Add(splitCode[1] + "." + classNameWithoutScope);
				}
				return declaredStaticClassList;
			}

			/// <summary>
			/// Gets the entire variable list that are declared in the code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private Dictionary<string, string> GetDeclaredVarList(string[] codes) {
				// Get all class names
				string[] declarationKeywordList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);
				for(int i = 0; i < declarationKeywordList.Length; i++) declarationKeywordList[i] += " ";
				List<string> listClassNames = declarationKeywordList.ToList<string>();
				List<string> listClassMemberKeywords = ClassMemberReservedKeyWords.ToList<string>();
				listClassMemberKeywords.InsertRange(0, listClassNames);
				declarationKeywordList = listClassMemberKeywords.ToArray<string>();
				// Get all variable declarations and store in a Dictionary List (key = variable name, value = bit size)
				Dictionary<string, string> declaredVarList = new Dictionary<string, string>();
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(!declarationKeywordList.Contains<string>(splitCode[0] + " ")) continue;
					string varTypeBit = ConvertVarTypeToASMPrefix(splitCode[0] + " ");
					if(varTypeBit == "") {
						// If this is a variable that points to class instance, then it is default to the maximum bit size
						if(listClassNames.Contains(splitCode[0] + " ")) varTypeBit = "u" + TARGET_ARCH_BIT_SIZE;
					}
					if(varTypeBit == "") continue;
					declaredVarList.Add(splitCode[1], varTypeBit);
				}
				return declaredVarList;
			}

			/// <summary>
			/// Get the bit size from the variable keyword type
			/// </summary>
			/// <param name="varType"></param>
			/// <returns></returns>
			private int GetBitSizeOfVarDeclaration(string varType) {
				int bitSize = 0;
				switch(varType) {
					case PH_RESV_KW_PTR: bitSize = TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_VAR: bitSize = TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_OBJECT: bitSize = TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_CHAR: bitSize = CHAR_BIT_SIZE; break;
					case PH_RESV_KW_STRING: bitSize = TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_BOOL: bitSize = BYTE_BIT_SIZE; break;
					case PH_RESV_KW_SBYTE: bitSize = BYTE_BIT_SIZE; break;
					case PH_RESV_KW_SHORT: bitSize = SHORT_BIT_SIZE; break;
					case PH_RESV_KW_INT: bitSize = INT_BIT_SIZE; break;
					case PH_RESV_KW_LONG: bitSize = LONG_BIT_SIZE; break;
					case PH_RESV_KW_BYTE: bitSize = BYTE_BIT_SIZE; break;
					case PH_RESV_KW_USHORT: bitSize = SHORT_BIT_SIZE; break;
					case PH_RESV_KW_UINT: bitSize = INT_BIT_SIZE; break;
					case PH_RESV_KW_ULONG: bitSize = LONG_BIT_SIZE; break;
					case PH_RESV_KW_FLOAT: bitSize = FLOAT_BIT_SIZE; break;
					case PH_RESV_KW_DOUBLE: bitSize = DOUBLE_BIT_SIZE; break;
					case PH_RESV_KW_DECIMAL: bitSize = DECIMAL_BIT_SIZE; break;
					default: break;
				}
				return bitSize;
			}

			/// <summary>
			/// Get the ASM prefix from the variable type that represents signed/unsigned 
			/// e.g.
			/// int ---> REGs32
			/// uint ---> REGu32
			/// </summary>
			/// <param name="varType"></param>
			/// <returns></returns>
			private string ConvertVarTypeToASMPrefix(string varType, string OpCode = "") {
				string asmCode = "";
				switch(varType) {
					case PH_RESV_KW_PTR: asmCode = OpCode + "u" + TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_VAR: asmCode = OpCode + "s" + TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_OBJECT: asmCode = OpCode + "u" + TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_CHAR: asmCode = OpCode + "u" + CHAR_BIT_SIZE; break;
					case PH_RESV_KW_STRING: asmCode = OpCode + "u" + TARGET_ARCH_BIT_SIZE; break;
					case PH_RESV_KW_BOOL: asmCode = OpCode + "u" + BYTE_BIT_SIZE; break;
					case PH_RESV_KW_SBYTE: asmCode = OpCode + "s" + BYTE_BIT_SIZE; break;
					case PH_RESV_KW_SHORT: asmCode = OpCode + "s" + SHORT_BIT_SIZE; break;
					case PH_RESV_KW_INT: asmCode = OpCode + "s" + INT_BIT_SIZE; break;
					case PH_RESV_KW_LONG: asmCode = OpCode + "s" + LONG_BIT_SIZE; break;
					case PH_RESV_KW_BYTE: asmCode = OpCode + "u" + BYTE_BIT_SIZE; break;
					case PH_RESV_KW_USHORT: asmCode = OpCode + "u" + SHORT_BIT_SIZE; break;
					case PH_RESV_KW_UINT: asmCode = OpCode + "u" + INT_BIT_SIZE; break;
					case PH_RESV_KW_ULONG: asmCode = OpCode + "u" + LONG_BIT_SIZE; break;
					case PH_RESV_KW_FLOAT: asmCode = OpCode + "s" + FLOAT_BIT_SIZE; break;
					case PH_RESV_KW_DOUBLE: asmCode = OpCode + "s" + DOUBLE_BIT_SIZE; break;
					case PH_RESV_KW_DECIMAL: asmCode = OpCode + "s" + DECIMAL_BIT_SIZE; break;
					default: break;
				}
				return asmCode;
			}

			private string[] ResolveStandAloneIncrementDecrement(string[] codes) {
				// Resolve stand-alone increment/decrement first
				List<string> codeList = codes.ToList<string>();
				for(int i = 0; i < codeList.Count; i++) {
					// Ignore expressions
					if(codeList[i] == "=") {
						for(int j = i; j < codeList.Count; j++) {
							if(codeList[j] == ";") {
								i = j;
								break;
							}
						}
						continue;
					}
					// Standalone increment/decrement does not matter whether it is pre-post
					// We just simply add/subtract 1
					if(codeList[i].IndexOf("＋") != -1) {
						string variable = "";
						// If we have an end of statement semicolon ";" after the + sign, it means it is a post increment
						// So variable should be found in the code before
						if(codeList[i + 1] == ";") {
							variable = codeList[i - 1];
							codeList[i] = "=";
							codeList.Insert(i + 1, variable);
							codeList.Insert(i + 2, "+");
							codeList.Insert(i + 3, "1");
						} else {
							// Otherwise, its a pre-increment
							variable = codeList[i + 1];
							codeList[i] = variable;
							codeList.Insert(i + 1, "=");
							codeList.Insert(i + 3, "+");
							codeList.Insert(i + 4, "1");
						}
						// Rewind index counter to the beginning of the new expression so we can process the newly added code
						i -= 2;
						continue;
					} else if(codeList[i].IndexOf("—") != -1) {

						string variable = "";
						// If we have an end of statement semicolon ";" after the - sign, it means it is a post increment
						// So variable should be found in the code before
						if(codeList[i + 1] == ";") {
							variable = codeList[i - 1];
							codeList[i] = "=";
							codeList.Insert(i + 1, variable);
							codeList.Insert(i + 2, "-");
							codeList.Insert(i + 3, "1");
						} else {
							// Otherwise, its a pre-increment
							variable = codeList[i + 1];
							codeList[i] = variable;
							codeList.Insert(i + 1, "=");
							codeList.Insert(i + 3, "-");
							codeList.Insert(i + 4, "1");
						}
						// Rewind index counter to the beginning of the new expression so we can process the newly added code
						i -= 2;
						continue;
					}
				}
				codes = codeList.ToArray();
				return codes;
			}

			/// <summary>
			/// Convert mathmatical expressions to ASM code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertExpressionsToASM(string[] codes, Dictionary<string, string> declaredVarList, Dictionary<string, string> instanceObjList, Dictionary<string, string> declaredFuncList) {
				// Get all classes
				string[] classDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);
				// For all codes, search for the assignemnt symbol
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != "=") continue;
					string expression = GetFullExpressionStatementFromAssignmentOperator(codes, i, true);
					codes[i] = ConvertExpressionStatementToASM(expression, codes, declaredVarList, instanceObjList, declaredFuncList, classDeclarationList);
				}
				return codes;
			}

			/// <summary>
			/// Given a token separated line of codes, this method returns the assignment operation statement, given the index where the assignemnt operator is found in the code array
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="assignmentOperatorIndex"></param>
			/// <param name="isRemoveExpressionFromCode">Determines if</param>
			/// <returns></returns>
			private string GetFullExpressionStatementFromAssignmentOperator(string[] codes, int assignmentOperatorIndex, bool isRemoveExpressionFromCode = false) {
				// If equal sign found, get the full expression statement
				int expEndIndex = -1;
				int expStartIndex = -1;
				for(int j = assignmentOperatorIndex; j < codes.Length; j++) {
					if(codes[j] == ";") {
						expEndIndex = j - 1;
						break;
					}
				}
				for(int j = assignmentOperatorIndex; j >= 0; j--) {
					if(Regex.IsMatch(codes[j], @"[;|\(|\)|{|}]")) {
						expStartIndex = j + 1;
						break;
					}
				}
				string expression = "";
				for(int j = expStartIndex; j <= expEndIndex; j++) {
					expression += codes[j];
					if(isRemoveExpressionFromCode) codes[j] = "";
				}
				return expression;
			}

			/// <summary>
			/// Find and resolve any post/pre increment/decrement
			/// </summary>
			/// <param name="splitCode"></param>
			/// <returns></returns>
			private string[] ResolveIncDecInExpression(string[] splitCode, Dictionary<string, string> declaredVarList, int offset = 0) {
				// Find and replace any post/pre increment/decrement
				List<string> splitCodeList = splitCode.ToList<string>();
				for(int i = 0; i < splitCodeList.Count; i++) {
					int indexInc = splitCodeList[i].IndexOf("＋");
					int indexDec = splitCodeList[i].IndexOf("—");
					if(indexInc == -1 && indexDec == -1) continue;
					if(indexInc != -1) {

						string variable = splitCodeList[i].Replace("＋", "");
						if(splitCodeList[i].Substring(0, 1) == "＋") {
							splitCodeList[i] = variable;
							// It's a pre increment
							splitCodeList.Insert(i - 1 - offset, PH_VAR + declaredVarList[variable] + " " + variable + " " + PH_ADD + " 1 ");
						} else {
							// It's a post increment
							splitCodeList[i] = variable;
							splitCodeList.Insert(i + 1, PH_VAR + declaredVarList[variable] + " " + variable + " " + PH_ADD + " 1 ");
						}
					} else {
						string variable = splitCodeList[i].Replace("—", "");
						if(splitCodeList[i].Substring(0, 1) == "—") {
							splitCodeList[i] = variable;
							// It's a pre decrement
							splitCodeList.Insert(i - 1 - offset, PH_VAR + declaredVarList[variable] + " " + variable + " " + PH_SUB + " 1 ");
						} else {
							// It's a post decrement
							splitCodeList[i] = variable;
							splitCodeList.Insert(i + 1, PH_VAR + declaredVarList[variable] + " " + variable + " " + PH_SUB + " 1 ");
						}
					}
				}
				splitCode = splitCodeList.ToArray();
				return splitCode;
			}

			/// <summary>
			/// Converts a single expression statement to ASM code
			/// </summary>
			/// <param name="expression"></param>
			/// <returns></returns>
			private string ConvertExpressionStatementToASM(string expression, string[] codes, Dictionary<string, string> declaredVarList, Dictionary<string, string> instanceObjList, Dictionary<string, string> declaredFuncList, string[] classDeclarationList) {
				// Get all classes
				//string[] classDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);
				// Temporary store the original expression
				string originalExpression = expression;
				// Ignore expressions inside double quotations (if any) - which are RPN equations
				string expRPN = Regex.Match(expression, @"\""(.*?)\""").Value;
				if(expRPN != "") expression = expression.Replace(expRPN, PH_ID + PH_EXP_STR + PH_ID);
				//expression = expression.Replace(PH_RESV_KW_VAR, " " + PH_VAR + TARGET_ARCH_BIT_SIZE + " ");
				string[] operatorList = { "==", "<=", ">=", "!=", "++", "--", "<", ">", "+", "-", "*", "/", "%", "&&", "||", "&", "|", "^", "=" };
				expression = expression.Replace("＋", PH_ID + "INCREMENT").Replace("—", PH_ID + "DECREMENT");
				for(int i = 0; i < operatorList.Length; i++) {
					expression = expression.Replace(operatorList[i], " " + ConvertOperatorToASM(operatorList[i]) + " ");
				}
				expression = expression.Replace(PH_ID + "INCREMENT", "＋").Replace(PH_ID + "DECREMENT", "—");

				// Split code up, and convert variable assignment to "VAR{u|s}xxx varname" format
				string[] splitCode = expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

				// Find and resolve any post/pre increment/decrement
				splitCode = ResolveIncDecInExpression(splitCode, declaredVarList, 1);

				for(int i = 0; i < splitCode.Length; i++) {
					// Remove pointer indicator for now
					string varToCheck = splitCode[i].Replace(PH_ID + PH_POINTER + PH_ID, "");
					// Remove square brackets for now
					varToCheck = Regex.Replace(varToCheck, @"\[(.*?)\]$", "");

					//string pointerIdentifier = "";
					if(!declaredVarList.ContainsKey(varToCheck)) {
						// No variable found in our declared list, so now we check if this variable is an instance object
						string checkInstanceObj = "";
						string[] splitVar = varToCheck.Split('.');
						if(splitVar.Length == 1) continue;
						for(int j = 0; j < splitVar.Length - 1; j++) checkInstanceObj += splitVar[j] + ".";
						checkInstanceObj = checkInstanceObj.Substring(0, checkInstanceObj.Length - 1);
						string resolvedClassScopeObj = checkInstanceObj;
						// Now check again with the resolved scope
						if(!instanceObjList.ContainsKey(checkInstanceObj)) {
							// If the variable does not exist in the instance object list, it might be a reference to the member of the class
							// So, resolve the entire function call scope path to absolute class scope path
							resolvedClassScopeObj = GetResolvedNestedInstanceVar(checkInstanceObj, instanceObjList);
							//varToCheck = checkInstanceObj;
							if(!instanceObjList.ContainsKey(resolvedClassScopeObj)) continue;
						}
						// It was an instance object, so now we check if the original variable exists as a class member variable
						varToCheck = ReplaceFirstOccurance(varToCheck, checkInstanceObj, instanceObjList[resolvedClassScopeObj]);

						if(!declaredVarList.ContainsKey(varToCheck)) continue;
						// Variable exists! We need to set this a pointer
						//pointerIdentifier = PH_ID + PH_POINTER + PH_ID;
						splitCode[i] = PH_VAR + declaredVarList[varToCheck] + " " + PH_ID + PH_POINTER + PH_ID + splitCode[i];
						break;
					}
					// If we have a variable that matches with our variable list item, get the bit size from the list and convert to ASM code
					//splitCode[i] = PH_VAR + declaredVarList[varToCheck] + " " + pointerIdentifier + splitCode[i];
					splitCode[i] = PH_VAR + declaredVarList[varToCheck] + " " + splitCode[i];
					break;
				}
				expression = "";
				for(int i = 0; i < splitCode.Length; i++) expression += splitCode[i] + " ";
				expression = expression.Trim();

				// RPN expressions can be complex like:
				// x = (a == b || c && d) + result_pne;
				// ---->
				// x = "a#b#==#c#d#&&#||#result_pne#+";
				if(expRPN != "") {
					expRPN = expRPN.Replace("\"", "");
					string[] splitExpRPN = expRPN.Split('#');
					// Check to see if any instance variables are used
					// If we do, add a pointer symbol
					for(int i = 0; i < splitExpRPN.Length; i++) {
						if(splitExpRPN[i].IndexOf(".") == -1) continue;
						string[] checkVarSplit = splitExpRPN[i].Split('.');
						string varToCheck = "";
						for(int j = 0; j < checkVarSplit.Length - 1; j++) {
							varToCheck += checkVarSplit[j] + ".";
						}
						varToCheck = varToCheck.Substring(0, varToCheck.Length - 1);
						if(classDeclarationList.Contains<string>(varToCheck)) {
							splitExpRPN[i] = PH_ID + PH_POINTER + PH_ID + splitExpRPN[i];
						}
					}

					string asmCode = "";
					//string[] regVarType = new string[2];
					for(int i = 0; i < splitExpRPN.Length; i++) {
						if(Operators.Contains<string>(splitExpRPN[i])) {
							if(splitExpRPN[i] == "!" || splitExpRPN[i] == "~" || splitExpRPN[i] == "UnaryMinus" || splitExpRPN[i] == "UnaryPlus") {
								asmCode += " " + PH_POP + " " + REG_SVAR0 + " " +
									PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + " " + REG_SVAR0 + " " +
									ConvertOperatorToASM(splitExpRPN[i]) + " " + PH_PSH + " " + REG_SVAR0 + " ";
							} else {
								// NOTE: Stack is First In Last Out (FILO), so we reverse variable orders to POP
								asmCode += " " + PH_POP + " " + REG_SVAR1 + " " + PH_POP + " " + REG_SVAR0 + " " +
									PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + " " + REG_SVAR0 + " " +
									ConvertOperatorToASM(splitExpRPN[i]) + " " + REG_SVAR1 + " " + PH_PSH + " " + REG_SVAR0 + " ";
							}
							//regVarType[0] = "";
							//regVarType[1] = "";
						} else {
							asmCode += PH_PSH + " " + splitExpRPN[i] + " ";
							/*if(regVarType[0] == "") {
								regVarType[0] = REG_VAR_LONG0;
							} else {
								regVarType[1] = REG_VAR_LONG1;
							}*/
						}
					}
					// Store the entire RPN calculation results into REG_VAR0
					asmCode = asmCode + PH_POP + " " + REG_SVAR0 + " ";
					expression = expression.Replace(PH_ID + PH_EXP_STR + PH_ID, REG_SVAR0);
					expression = asmCode + expression;

					// Find and resolve any post/pre increment/decrement
					string[] splitCodeExpression = expression.Split(' ');
					splitCodeExpression = ResolveIncDecInExpression(splitCodeExpression, declaredVarList, 0);
					expression = "";
					for(int i = 0; i < splitCodeExpression.Length; i++) expression += splitCodeExpression[i] + " ";
				}

				// If expression contains sequential function calls
				if(expression.IndexOf("#") != -1) {
					// TODO: Sequential function calls
				} else if(expression.IndexOf(")") != -1) {
					// If expression contains stand alone function call
					// Note that we will only have one function call per statement (as it was already pre-processed to contain one func call per statement)
					int funcEndIndex = expression.IndexOf(")");
					int funcStartIndex = ObtainFuncVarStartIndex(expression, funcEndIndex, "(", false);
					string function = expression.Substring(funcStartIndex, funcEndIndex - funcStartIndex + 1);
					// Now check if there are any parameters
					string asmParamPushCode = "";
					if(function.IndexOf(",") != -1) {
						// This function contains multiple parameters
						string[] funcParams = Regex.Match(function, @"(?<=\().+?(?=\))").Value.Split(',');
						for(int i = 0; i < funcParams.Length; i++) {
							asmParamPushCode += PH_PSH + " " + funcParams[i] + " ";
						}
						//asmParamPushCode = 
					} else if(function.IndexOf("()") == -1) {
						// This function contains a single parameter
						string funcParam = Regex.Match(function, @"(?<=\().+?(?=\))").Value;
						asmParamPushCode = PH_PSH + " " + funcParam + " ";
					} else {
						// This function contains no parameters (therefore, no need to construct parameter PUSH ASM code)
					}
					string functionName = Regex.Replace(function, @"\((.*?)\)", "");

					string retVarType = "";
					// If expression contains NEW keyword, this function call is a call to the class initiator, so return value will be an unsigned long
					if(expression.IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
						retVarType = REG_UVAR0;
					} else {
						// Otherwise, return signed long var
						retVarType = REG_SVAR0;
					}
					// If function return type is a pointer or an unsigned long, then return type will be unsigned long
					string[] splitFuncName = functionName.Split('.');
					string chkFunctionName = "";
					for(int i = 0; i < splitFuncName.Length - 1; i++) chkFunctionName += splitFuncName[i] + ".";
					chkFunctionName = chkFunctionName.Substring(0, chkFunctionName.Length - 1);

					// Resolve the entire function call scope path to absolute class scope path
					//chkFunctionName = GetResolvedNestedInstanceVar(chkFunctionName, instanceObjList);

					string funcWithRealClassName = "";
					if(instanceObjList.ContainsKey(chkFunctionName)) {
						funcWithRealClassName = instanceObjList[chkFunctionName];
						funcWithRealClassName = funcWithRealClassName + "." + splitFuncName[splitFuncName.Length - 1];
						if(declaredFuncList.ContainsKey(funcWithRealClassName)) {
							if(declaredFuncList[funcWithRealClassName] + " " == PH_RESV_KW_FUNCTION_PTR) {
								retVarType = REG_UVAR0;
							} else if(declaredFuncList[funcWithRealClassName] + " " == PH_RESV_KW_FUNCTION_ULONG) {
								retVarType = REG_UVAR0;
							}
						}
					} else if(declaredFuncList.ContainsKey(functionName)) {
						if(declaredFuncList[functionName] + " " == PH_RESV_KW_FUNCTION_PTR) {
							retVarType = REG_UVAR0;
						} else if(declaredFuncList[functionName] + " " == PH_RESV_KW_FUNCTION_ULONG) {
							retVarType = REG_UVAR0;
						}
					}



					//PSH iajhl JMS N.Program.Main.i.SetA POP REG_SVAR0 PSH iajhl JMS N.Program.Main.i.SetA POP REG_SVAR0 REG_SVAR0 MOV   REG_SVAR0 REG_SVAR0
					//expression = asmParamPushCode + PH_JMS + " " + functionName + " " + PH_POP + " " + REG_VAR0 + " " + expression.Replace(function, "") + " " + REG_VAR0;
					expression = asmParamPushCode + PH_JMS + " " + functionName + " " + PH_POP + " " + retVarType + " " + expression.Replace(function, "") + " " + retVarType;
				} else if(expression.IndexOf("]") != -1) {

					// Instantiating an array of objects, which means we do not have any parameters to 
					// (e.g.) VARu64 N.Program.Main.b MOV 𒀭PH_NEW𒀭N.INT32[xxxxx]
					// (where xxxxx is the number of arrays to create
					int funcEndIndex = expression.IndexOf("]");
					int funcStartIndex = ObtainFuncVarStartIndex(expression, funcEndIndex, "[", false);
					string instanceObjWithParam = expression.Substring(funcStartIndex, funcEndIndex - funcStartIndex + 1);
					string instanceObjWithoutParam = Regex.Replace(instanceObjWithParam, @"\[(.*?)\]", "");
					string instanceObjName = instanceObjWithoutParam.Replace(PH_ID + PH_NEW + PH_ID, "");
					string arrayItem = Regex.Match(instanceObjWithParam, @"(?<=\[).+?(?=\])").Value;

					// If classWithoutParam does not contain a NEW keyword, this is not an instantiation, but just an array index reference
					if(instanceObjWithoutParam.IndexOf(PH_ID + PH_NEW + PH_ID) == -1) {
						// Get the size of this array (it will be an instance variable)
						string instanceVar = instanceObjName;
						// Replace any pointer keywords
						instanceVar = instanceVar.Replace(PH_ID + PH_POINTER + PH_ID, "");
						if(instanceObjList.ContainsKey(instanceVar)) {
							instanceObjName = instanceObjList[instanceVar];
						} else {
							// If the variable does not exist in the instance object list, it might be a reference to the member of the class
							// So, resolve the entire function call scope path to absolute class scope path
							string[] splitFuncName = instanceVar.Split('.');
							string chkFunctionName = "";
							for(int i = 0; i < splitFuncName.Length - 1; i++) chkFunctionName += splitFuncName[i] + ".";
							chkFunctionName = chkFunctionName.Substring(0, chkFunctionName.Length - 1);
							if(!instanceObjList.ContainsKey(chkFunctionName)) {
								chkFunctionName = GetResolvedNestedInstanceVar(chkFunctionName, instanceObjList);
							}
							instanceObjName = instanceObjList[chkFunctionName];
						}
						// Create Memory allocation command for the number of arrays
						//int biteSize = CalculateClassMemberVarSize(codes, instanceObjName);
						// Note that if a new array is created for a non-class (primitive types like int, byte, etc) we will need to get the primitive size
						//if(biteSize == 0) biteSize = GetBitSizeOfVarDeclaration(instanceObjName + " ") / 8;

						int byteSize = 0;
						if(instanceObjName + " " == PH_RESV_KW_PTR) {
							// If instanceObjName has "ptr𒀭" as object type, we don't really know the bytesize for it
							// We will find it out later
							byteSize = -1;
						} else {
							// Always the byte size of the class pointer / primitive size
							byteSize = GetBitSizeOfVarDeclaration(instanceObjName + " ") / 8;
							// If byteSize is zero, it means we are dealing with a class, so we assign the class pointer size
							if(byteSize == 0) byteSize = TARGET_ARCH_BIT_SIZE / 8;
						}

						// TODO: If we are going to support multi-dimensional arrays, we will need to code it here...
						// ____________________________________________
						// In our expression, we can assume that there will be only one instance of array access, since we have organized the expression to be so
						// Hence, there are two types of expressions that operates on an array
						// ===============================
						// (e.g.) Assign Value To Array
						// ===============================
						// X.TEST.A.arr[ocy0,ocy1] MOV VARu8 final_qhu					; arr[1, 2] = 0;  {WHERE: int[,] arr = new int[3, 4];}
						// N.Program.Main.b[nqn] MOV VARu64 final_opf						; b[0] = new INT32();  {WHERE: INT32[] b = new INT32[10];}
						// N.Program.Main.arr[msu0,msu1] MOV VARu64 final_oer		; arr[1,1] = new int();  {WHERE: int[,] arr = new int[20,30];}
						// ===============================
						// (e.g.) Assign From Array value
						// ===============================
						// VARs32 X.TEST.A.a MOV X.TEST.A.arr[ykb0,ykb1]				; int a = arr[1, 2];
						// VARs64 final_kct MOV X.TEST.A.asd[sxs0,sxs1,sxs2]		; arr[1, 2] = asd[1, 2, 3];
						// 
						// bool results = arr[1, 2] == asd[1, 1, 1];
						// 
						// TODO: Get the memory array index
						// ____________________________________________
						// We can calculate the index of a N-Dimensional array in our 1-Dimensional array space as:
						// new array[X, Y, Z];
						// ---->
						// MaxArraySize = X * Y * Z
						// ArrayIndexIn1DMemory =
						// X * MaxArraySize / MaxOf(X)
						// +
						// Y * MaxArraySize / MaxOf(X) / MaxOf(Y)
						// +
						// Z * MaxArraySize / MaxOf(X) / MaxOf(Y) / MaxOf(Z)
						// + ...... and so on until the Nth dimension
						// ____________________________________________
						// TODO: Assign Value To Array
						// TODO: Assign From Array Value



						// Convert array code referencing/assignment to array
						// (e.g.)
						// ### VARu64 final_ytp MOV 0 MAL 8 PSH final_ytp
						// ### JMS N.INT32 JMS N.INT32.INT32 POP REG_VAR0 POP REG_VAR0 VARu64 final_ytp MOV   REG_VAR0
						// VARu64 N.Program.Main.b[gmi] MOV final_ytp					; b[0] = new INT32();
						// -----
						// VARu64 N.Program.Main.c[iqt] MOV final_hvj 				; c[1] = 100;
						// -----
						// VARs32 N.Program.Main.x MOV N.Program.Main.c[dkn] 	; int x = c[0];
						// ------
						// VARs64 result_gre MOV N.Program.Main.c[gre]				; bool results = c[1] == c[0];
						// VARs64 result_izm MOV N.Program.Main.c[izm]
						// ### N.Program.Main.results = "result_gre#result_izm#=="
						// ------
						// VARs64 final_bhc MOV N.Program.Main.c[itg]					; c[0] = c[1];
						// VARu64 N.Program.Main.c[blu] MOV final_bhc 
						string[] splitArrayCode = expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

						// We might have something like:
						// [0] result_gcexxyiq
						// [1] MOV
						// [2] VARu32
						// [3] N.Program.Main.i[gcexxyiq]
						// In this case, we need to remove [2]
						for(int i = 0; i < splitArrayCode.Length; i++) {
							if(splitArrayCode[i] != PH_MOV) continue;
							if(Regex.IsMatch(splitArrayCode[i + 1], PH_VAR + @"[u|s][0-9]")) {
								splitArrayCode[i + 1] = "";
								expression = "";
								for(int j = 0; j < splitArrayCode.Length; j++) expression += splitArrayCode[j] + " ";
								splitArrayCode = expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
							}
						}

						for(int i = 0; i < splitArrayCode.Length; i++) {
							if(splitArrayCode[i].IndexOf("[") == -1) continue;

							// Note that instanceObjWithoutParam might be a nested pointer so we might need to reference the address it points to by adding a # sign in front of the instanceObjWithoutParam
							// ----------------------------------------------------------
							// e.g. Nested pointer
							// class Main { INT32 i = new INT32(); i.setA(10); }
							// class INT32 { byte[] a = new byte[2]; void setA(byte val) { a[0] = val; } }  <-------- "a" is an array within an instance (which is a pointer within a pointer)
							// ----------------------------------------------------------
							// So, first of all we need to get all classes inside our instance object list
							List<string> listOfClass = new List<string>();
							foreach(var kvp in instanceObjList) {
								if(listOfClass.Contains(kvp.Value)) continue;
								if(ClassMemberReservedKeyWords.Contains<string>(kvp.Value + " ")) continue;
								listOfClass.Add(kvp.Value);
							}
							bool isNestedPointerObj = false;
							foreach(string className in listOfClass) {
								if(instanceObjWithoutParam.Length < className.Length) continue;
								if(instanceObjWithoutParam == className) {
									isNestedPointerObj = true;
									break;
									// ADDED 2022/03/25
								} else if((instanceObjWithoutParam.Length >= className.Length + 1) && (instanceObjWithoutParam.Substring(0, className.Length + 1) == className + ".")) {
									isNestedPointerObj = true;
									break;
								}
							}

							// We have an address passed on to a function scope variable, so we need to get the type of pointer here
							if(byteSize == -1) {
								isNestedPointerObj = false;
								if(ActualClassMemberPointerVarTypeList.ContainsKey(instanceObjWithoutParam)) {
									byteSize = GetBitSizeOfVarDeclaration(ActualClassMemberPointerVarTypeList[instanceObjWithoutParam] + " ") / 8;
								} else {
									byteSize = TARGET_ARCH_BIT_SIZE / 8;
								}
							}

							string referAddressString = "";
							if(isNestedPointerObj) referAddressString = "#";

							// If array variable has a variable operation operand "VARxxx" in front of it, then it's an assignment to an array variable
							if(Regex.IsMatch(splitArrayCode[i - 1], PH_VAR + @"[u|s][0-9]")) {
								string asmArrayAssignCode = REG_UVAR0 + " " + PH_MOV + " " + arrayItem + " " + PH_MLT + " " + byteSize + " " + PH_ADD + " " + referAddressString + instanceObjWithoutParam;
								splitArrayCode[i] = asmArrayAssignCode;
								splitArrayCode[i + 1] = PH_PSH + " " + splitArrayCode[i + 2];

								// If instanceObjWithoutParam is declared as a pointer, we just use the value of REG_UVAR0
								if(instanceObjList[instanceObjWithoutParam] + " " == PH_RESV_KW_PTR) {
									splitArrayCode[i + 2] = PH_POP + " " + REG_UVAR0;
								} else {
									splitArrayCode[i + 2] = PH_POP + " " + PH_ID + PH_POINTER + PH_ID + REG_UVAR0;
								}

								//splitArrayCode[i + 2] = PH_POP + " " + PH_ID + PH_POINTER + PH_ID + REG_UVAR0;
								expression = "";
								break;
							} else {
								// Else, the array is referenced by some other operation (for example, a function call: "X[0].CallFunction();", or variable assignments: "int xxx = X[0]")
								string asmArrayAssignCode = PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + arrayItem + " " + PH_MLT + " " + byteSize + " " + PH_ADD + " " + referAddressString + instanceObjWithoutParam + " ";


								// If instanceObjWithoutParam is declared as a pointer, we just use the value of REG_UVAR0
								/*
								if(instanceObjList[instanceObjWithoutParam] + " " == PH_RESV_KW_PTR) {
									splitArrayCode[i] = REG_UVAR0;
								} else {
									splitArrayCode[i] = PH_ID + PH_POINTER + PH_ID + REG_UVAR0;
								}*/
								splitArrayCode[i] = PH_ID + PH_POINTER + PH_ID + REG_UVAR0;

								expression = asmArrayAssignCode;
								break;
							}
						}
						for(int j = 0; j < splitArrayCode.Length; j++) expression += splitArrayCode[j] + " ";
					} else {
						// Create Memory allocation command for the number of arrays
						int byteSize = CalculateClassMemberVarSize(codes, instanceObjName);
						// Note that if a new array is created for a non-class (primitive types like int, byte, etc) we will need to get the primitive size
						if(byteSize == 0) byteSize = GetBitSizeOfVarDeclaration(instanceObjName + " ") / 8;

						// If the array instantiated is user-defined class.. (if not primitive types)
						if(!ClassMemberReservedKeyWords.Contains<string>(instanceObjName + " ")) {
							// If classWithParam contains a NEW and square brackets (e.g. 𒀭PH_NEW𒀭N.INT32[tguhzwtc]), we can say that an instance is created as an array
							// Therefore, we can assume that multiple arrays are created which are just pointers (pointers have the maximum target architecutre byte size = TARGET_ARCH_BIT_SIZE / 8)
							if(instanceObjWithParam.IndexOf("[") != -1 && instanceObjWithParam.IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
								byteSize = TARGET_ARCH_BIT_SIZE / 8;
							}
						}

						string numArrayToCreate = arrayItem;
						// If array contains multi-dimentional array
						if(arrayItem.IndexOf(",") != -1) {
							string[] items = arrayItem.Split(',');
							arrayItem = "";
							for(int i = 0; i < items.Length; i++) {
								arrayItem += PH_MLT + " " + items[i] + " ";
							}
						} else {
							arrayItem = PH_MLT + " " + arrayItem + " ";
						}

						// ************************************************************************
						// If allocating virtual memory in one call (for performance), use this line:
						// ************************************************************************
						/*
						string newArrayASMCode = PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + byteSize + " " + arrayItem +
							ReplaceFirstOccurance(expression, classWithParam, "0") + " " +
							//PH_MAL + " " + REG_UVAR0;
							PH_PSH + " " + REG_UVAR0 + " " + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " + PH_POP + " " + REG_SVAR0;
						*/


						/* ------
						 * TEST: Adds extra memory space after creating the array, and add -1 (but this will not work)
						 * ------
						 * 
						// ************************************************************************
						// If allocating virtual memory one by one (for clarity during IASM code debugging), use this line:
						// ************************************************************************
						// (i.e. int[] i = new int[xxx])
						// REGu64 tmpvar VARu64 tmpvar MOV xxx ADD 1												; tmpvar holds the number of arrays to be created + extra 1 memory space to indicate the end of allocation block
						// VARu64 REG_UVAR1 MOV tmpvar MLT yyy 																; REG_UVAR1 stores the total bytes to be allocated (here, yyy is the byte size - it may vary)
						// VARu64 N.Program.Main.i MOV 0																		; The pointer variable to be assigned with the beginning address of the allocated memory (int[] i)
						// NOP lbl_tmpvar																										; loop label
						// VARu64 REG_UVAR0 MOV yyy																						; REG_UVAR0 stores the byte size, which is the parameter for the VMC - Malloc call
						// VARu64 N.Program.Main.i PSH REG_UVAR0 JMS Malloc POP REG_SVAR0		; Call VMC Malloc call, and N.Program.i will be assigned with the newly created address automatically
						// VARu64 tmpvar SUB 1 SKE 0 JMP lbl_tmpvar													; if(tmpvar - 1 != 0) goto lbl_tmpvar
						// VARu64 N.Program.Main.i SUB REG_UVAR1 ADD yyy										; N.Program.Main.i stores the last memory address allocated,
						//																																	; so we simply subtract the total bytes allocated from it to get the beginning of the address,
						//																																	; with padding extra buffer (4 - for extra byte in this case) since the new address pointer will be added with extra amount
						// VARu64 REG_UVAR1 MOV xxx ADD 1 MLT yyy														; assign the address of the instance var, and add the number of created arrays (in bytes)
						// VARu64 REG_UVAR0 MOV N.Program.Main.i ADD REG_UVAR1 SUB yyy
						// VARu64 #REG_UVAR0 SUB 1																					; Make the last extra memory space -1 (null) to indicate this is the end of allocated memory block
						string instanceObjCode = ReplaceFirstOccurance(expression, classWithParam, "0");
						// Create a new variable to store the function call sequence results and insert into the expression list
						string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true);
						string newArrayASMCode =
							PH_REG + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_MOV + " " + numArrayToCreate + " " + PH_ADD + " 1 " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR1 + " " + PH_MOV + " " + tmpVar + " " + PH_MLT + " " + byteSize + " " +
							instanceObjCode + " " + PH_NOP + " " + "lbl_" + tmpVar + ";";
						instanceObjCode = instanceObjCode.Split(' ')[0] + " " + instanceObjCode.Split(' ')[1];
						newArrayASMCode +=
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + byteSize + " " +
							instanceObjCode + " " + PH_PSH + " " + REG_UVAR0 + " " + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " + PH_POP + " " + REG_SVAR0 + " " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_SUB + " 1 " + PH_SKE + " 0 " + PH_JMP + " lbl_" + tmpVar + " " +
							instanceObjCode + " " + PH_SUB + " " + REG_UVAR1 + " " + PH_ADD + " " + byteSize + " " +

							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR1 + " " + PH_MOV + " " + numArrayToCreate + " " + PH_ADD + " 1 " + PH_MLT + " " + byteSize + " " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + instanceObjCode.Split(' ')[1] + " " + PH_ADD + " " + REG_UVAR1 + " " + PH_SUB + " " + byteSize + " " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + PH_ID + PH_POINTER + PH_ID + REG_UVAR0 + " " + PH_SUB + " 1 ";
						*/



						// ************************************************************************
						// If allocating virtual memory one by one (for clarity during IASM code debugging), use this line:
						// ************************************************************************
						// (i.e. int[] i = new int[xxx])
						// REGu64 tmpvar VARu64 tmpvar MOV xxx															; tmpvar holds the number of arrays to be created, which will be decremented after each memory allocation
						// VARu64 REG_UVAR1 MOV tmpvar MLT 4 																; REG_UVAR1 stores the total bytes to be allocated
						// VARu64 N.Program.Main.i MOV 0																		; The pointer variable to be assigned with the beginning address of the allocated memory (int[] i)
						// NOP lbl_tmpvar																										; loop label
						// VARu64 REG_UVAR0 MOV 4																						; REG_UVAR0 stores the byte size, which is the parameter for the VMC - Malloc call
						// VARu64 N.Program.Main.i PSH REG_UVAR0 JMS Malloc POP REG_SVAR0		; Call VMC Malloc call, and N.Program.i will be assigned with the newly created address automatically
						// VARu64 tmpvar SUB 1 SKE 0 JMP lbl_tmpvar													; if(tmpvar - 1 != 0) goto lbl_tmpvar
						// VARu64 N.Program.Main.i SUB REG_UVAR1 ADD 4											; N.Program.Main.i stores the last memory address allocated,
						//																																	; so we simply subtract the total bytes allocated from it to get the beginning of the address,
						//																																	; with padding extra buffer (4 - for extra byte in this case) since the new address pointer will be added with extra amount
						string instanceObjCode = ReplaceFirstOccurance(expression, instanceObjWithParam, "0");
						// Create a new variable to store the function call sequence results and insert into the expression list
						string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
						string newArrayASMCode =
							PH_REG + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_MOV + " " + numArrayToCreate + " " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR1 + " " + PH_MOV + " " + tmpVar + " " + PH_MLT + " " + byteSize + " " +
							instanceObjCode + " " + PH_NOP + " " + "lbl_" + tmpVar + ";";
						instanceObjCode = instanceObjCode.Split(' ')[0] + " " + instanceObjCode.Split(' ')[1];

						newArrayASMCode +=
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + byteSize + " " +
							instanceObjCode + " " + PH_PSH + " " + REG_UVAR0 + " " + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " + PH_POP + " " + REG_SVAR0 + " " +
							PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + tmpVar + " " + PH_SUB + " 1 " + PH_SKE + " 0 " + PH_JMP + " lbl_" + tmpVar + " " +
							instanceObjCode + " " + PH_SUB + " " + REG_UVAR1 + " " + PH_ADD + " " + byteSize + " ";

						// Add code to call the VM's register malloc group size functionality - which tells the VM the beginning address of the allocated heap, and the total size of allocated bytes we created
						// This information is required by the VM so it can garbage collect efficiently
						newArrayASMCode += PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR0 + " " + PH_MOV + " " + numArrayToCreate + " " + PH_MLT + " " + byteSize + " " +
							//PH_PSH + " " + REG_UVAR0 + " " +
							//PH_PSH + " " + instanceObjCode.Split(' ')[1] + " " +	
							PH_PSH + " " + instanceObjCode.Split(' ')[1] + " " +
							PH_PSH + " " + REG_UVAR0 + " " +
							PH_JMS + " " + PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE + " " + PH_POP + " " + REG_SVAR0 + " "; // REMEMBER: Any JMS calls that do not return any value mush have "POP REG_UVAR0" to pop unwanted return 0 value in stack!!

						expression = newArrayASMCode;
					}
				}

				// If expression contains "JMS 𒀭PH_NEW𒀭XXXX", it means this is an instantiation code, so we need to construct the corresponding initiation ASM code
				instantiationCheck:
				int instantiateStartIndex = expression.IndexOf(PH_JMS + " " + PH_ID + PH_NEW + PH_ID);
				if(instantiateStartIndex != -1) {
					int instantiateEndIndex = expression.IndexOf(" ", instantiateStartIndex + PH_JMS.Length + 4);
					string instCurrentCode = expression.Substring(instantiateStartIndex, instantiateEndIndex - instantiateStartIndex);
					// Get the left side of the assignment operand (=), which is the object name to store the address of instantiated object address
					string newObjName = originalExpression.Substring(0, originalExpression.IndexOf("="));
					string className = Regex.Match(originalExpression, @"(?<=" + PH_ID + PH_NEW + PH_ID + @").+?(?=\()").Value;
					string[] splitClassName = className.Split('.');
					string constructorName = splitClassName[splitClassName.Length - 1];

					// ************************************************************************
					// If allocating virtual memory in one call (for performance), use this line:
					// ************************************************************************
					/*
					string instantiationASMCode = PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + newObjName + " " + PH_MOV + " 0 " +
						//PH_MAL + " " + CalculateClassMemberVarSize(codes, className) + " " +
						PH_PSH + " " + CalculateClassMemberVarSize(codes, className) + " " + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " + PH_POP + " " + REG_UVAR0 + " " +
						PH_PSH + " " + newObjName + " " + PH_JMS + " " + className + " " + PH_JMS + " " + className + "." + constructorName + " " + PH_POP + " " + REG_SVAR0;
					*/


					// ************************************************************************
					// If allocating virtual memory one by one (for clarity during IASM code debugging), use this line:
					// ************************************************************************
					// (i.e. INT32 i = new INT32();  -->  Where class INT32 { private int A; private byte B; })
					// VARu64 REG_UVAR1 MOV xxxx 																				; REG_UVAR1 stores the total member variable size (bytes) to be allocated
					// VARu64 N.Program.Main.i MOV 0																		; The pointer variable to be assigned with the beginning address of the allocated memory (INT32 i)
					// VARu64 N.Program.Main.i PSH 8																		; Class will always have an address (8 bytes in this case) to store its beginning address
					// JMS Malloc POP REG_SVAR0
					// VARu64 N.Program.Main.i PSH 4																		; The next member's memory allocation (in this case, an integer so 4 bytes allocated)
					// JMS Malloc POP REG_SVAR0
					// VARu64 N.Program.Main.i PSH 1																		; The next member's memory allocation (in this case, a byte so 1 byte allocated)
					// JMS Malloc POP REG_SVAR0
					// VARu64 N.Program.Main.i SUB REG_UVAR1 ADD 1											; N.Program.Main.i stores the last memory address allocated
					//																																	; so we simply subtract the total bytes allocated from it to get the beginning of the address,
					//																																	; with padding extra buffer (1 - for extra byte in this case) since the new address pointer will be added with the last member variable size
					//
					// PSH N.Program.Main.i JMS INT32																		; Call class's memory assignment routine (which will be the initiation block of the class which is automatically generated)
					// JMS INT32.INT32																									; Call class's constructor
					// POP REG_SVAR0																									; POP dummy value from stack (remember? all functions return 0 even they are void, so 0 is pushed in the stack and we need to pop it)
					//
					// POP REG_UVAR0 VARu64 N.Program.Main.i MOV REG_UVAR0							; This code is already in the expression, so we simply add the codes above to make it a complete code

					// Stores the size of each member variables (in bytes) in the specified class 
					//List<int> memberVarSizeList = GetClassMemberVarSizeList(codes, className);
					List<int> memberVarSizeList = ActualClassMemberVarSizeList[className];
					//int totalMemberVarSize = CalculateClassMemberVarSize(codes, className, memberVarSizeList);
					int totalMemberVarSize = 0;
					foreach(int size in memberVarSizeList) totalMemberVarSize += size;
					// Create code to memory allocate for each member size
					string instantiationASMCode = PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + REG_UVAR1 + " " + PH_MOV + " " + totalMemberVarSize + " " +
						PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + newObjName + " " + PH_MOV + " 0 ";
					foreach(int varSize in memberVarSizeList) {
						instantiationASMCode += PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + newObjName + " " + PH_PSH + " " + varSize + " " + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " + PH_POP + " " + REG_SVAR0 + " ";
					}

					// We need the instantiated object to point to the beginning of the newly created address (because currently the instance object points to the LAST address of the newly allocated memory)
					//instantiationASMCode += PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + newObjName + " " + PH_SUB + " " + totalMemberVarSize + " " + PH_ADD + " 1 ";


					instantiationASMCode += PH_VAR + "u" + TARGET_ARCH_BIT_SIZE + " " + newObjName + " " + PH_SUB + " " + REG_UVAR1 + " " + PH_ADD + " " + memberVarSizeList[memberVarSizeList.Count - 1] + " " +
						PH_PSH + " " + newObjName + " " + PH_JMS + " " + className + " " + PH_JMS + " " + className + "." + constructorName + " " + PH_POP + " " + REG_SVAR0 + " ";

					//VARu64 N.Program.Main.i MOV 0 PSH 13 JMS DScript.VMC.Malloc POP REG_UVAR0 PSH N.Program.Main.i JMS N.INT32 JMS N.INT32.INT32 POP REG_SVAR0       ---- POP REG_UVAR0 VARu64 N.Program.Main.i MOV   REG_UVAR0


					expression = ReplaceFirstOccurance(expression, instCurrentCode, instantiationASMCode);

					// Add code to call the VM's register malloc group size functionality - which tells the VM the beginning address of the allocated heap, and the total size of allocated bytes we created
					// This information is required by the VM so it can garbage collect efficiently
					expression +=
						//" " + PH_PSH + " " + totalMemberVarSize + " " +
						//PH_PSH + " " + newObjName + " " +
						" " + PH_PSH + " " + newObjName + " " +
						PH_PSH + " " + totalMemberVarSize + " " +
						PH_JMS + " " + PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE + " " + PH_POP + " " + REG_SVAR0 + " "; // REMEMBER: Any JMS calls that do not return any value mush have "POP REG_UVAR0" to pop unwanted return 0 value in stack!!

					// We might have multiple instantiations, so check again
					goto instantiationCheck;
				}

				return expression;
			}

			/// <summary>
			/// Claculates the total size (in bytes) of the member variables in the specified class
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="className"></param>
			/// <param name="memberVarSizeList">If specified, codes will be ignored and this list will be used instead</param>
			/// <returns></returns>
			private int CalculateClassMemberVarSize(string[] codes, string className, List<int> memberVarSizeList = null) {
				int total = 0;
				List<int> memberVarList = memberVarSizeList;
				if(memberVarList == null) {
					memberVarList = GetClassMemberVarSizeList(codes, className);
				}
				foreach(int varSize in memberVarList) {
					total += varSize;
				}
				return total;
			}

			/// <summary>
			/// Gets the size of each member variables (in bytes) in the specified class
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="className"></param>
			/// <returns></returns>
			private List<int> GetClassMemberVarSizeList(string[] codes, string className) {
				// Stores the total number of byte size this class has
				List<int> retList = new List<int>();

				int endClassScopeIndex = -1;
				int startClassScopeIndex = -1;

				// Goto class declaration
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != PH_RESV_KW_CLASS + className) continue;
					// Search for the first function, then break
					for(int j = i; j < codes.Length; j++) {
						//if(codes[j].IndexOf(PH_RESV_KW_FUNCTION, 0) == -1) continue;
						string[] splitFuncName = codes[j].Split(' ');
						if(!AllFunctionKeywords.Contains<string>(splitFuncName[0] + " ")) continue;
						endClassScopeIndex = j - 1;
						break;
					}
					// Find class contents start index
					for(int j = i; j < endClassScopeIndex; i++) {
						if(codes[i] == "{") {
							startClassScopeIndex = i + 1;
							break;
						}
					}
					break;
				}
				// Now get the total number of class member variable size within this class
				for(int i = startClassScopeIndex; i < endClassScopeIndex; i++) {
					string varDeclareCode = Regex.Match(codes[i], PH_REG + "(.*?) ").Value.Trim();
					if(varDeclareCode.IndexOf(PH_REG, 0) != -1) {
						try {
							int bitSize = Convert.ToInt32(varDeclareCode.Replace(PH_REG, "").Replace("u", "").Replace("s", ""));
							int byteSize = bitSize / 8;
							retList.Add(byteSize);
						} catch { }
					}
				}
				return retList;
			}

			private string ConvertOperatorToASM(string operatorStr) {
				string asmCode = "";
				switch(operatorStr) {
					case "UnaryMinus": asmCode = PH_UMN; break;
					case "UnaryPlus": asmCode = PH_UPL; break;
					//case "UnaryNot": asmCode = PH_UNT; break;
					//case "BitwiseUnaryComplement": asmCode = PH_BUC; break;
					case "==": asmCode = PH_LEQ; break;
					case "<=": asmCode = PH_LSE; break;
					case ">=": asmCode = PH_LGE; break;
					case "!=": asmCode = PH_LNE; break;
					case "<": asmCode = PH_LGS; break;
					case ">": asmCode = PH_LGG; break;
					case "+": asmCode = PH_ADD; break;
					case "-": asmCode = PH_SUB; break;
					case "*": asmCode = PH_MLT; break;
					case "/": asmCode = PH_DIV; break;
					case "%": asmCode = PH_MOD; break;
					case "<<": asmCode = PH_SHL; break;
					case ">>": asmCode = PH_SHR; break;
					case "&&": asmCode = PH_CAN; break;
					case "||": asmCode = PH_COR; break;
					case "&": asmCode = PH_AND; break;
					case "|": asmCode = PH_LOR; break;
					case "^": asmCode = PH_XOR; break;
					case "!": asmCode = PH_UNT; break;
					case "~": asmCode = PH_BUC; break;
					case "=": asmCode = PH_MOV; break;
					default: break;
				}
				return asmCode;
			}

			/// <summary>
			/// Converts variable declarations inside a single statement to ASM code
			/// </summary>
			/// <param name="code"></param>
			/// <param name="isConvertToPointerVar"></param>
			/// <returns></returns>
			private string ConvertVarDeclarationsToASM(string code, int previousAddressByteSize = 0, bool isConvertToPointerVar = false, string currentClassName = "") {
				string[] splitCode = code.Split(' ');
				if(splitCode.Length == 2) {
					string varType = splitCode[0] + " ";
					string varCmd = "";
					string ptrSymbol = isConvertToPointerVar ? " " + PH_ID + PH_POINTER + PH_ID : " ";
					string[] splitMemberVarName = splitCode[1].Split('.');
					string className = "";
					for(int i = 0; i < splitMemberVarName.Length - 1; i++) {
						className += splitMemberVarName[i] + ".";
					}
					// If class name exists, we get the class name
					// But if class name does not exist, then it's a temporary variable so we will store the current class where this variable belongs
					if(className != "") {
						className = className.Substring(0, className.Length - 1);
					} else {
						className = currentClassName;
					}
					// If class name was given as parameter (currentClassName), then we use it instead
					if(currentClassName != "") className = currentClassName;

					string varTypeBit = ConvertVarTypeToASMPrefix(varType);
					switch(varType) {
						case PH_RESV_KW_PTR: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_VAR: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_OBJECT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_STRING: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_CHAR: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_BOOL: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_SBYTE: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_SHORT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_INT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_LONG: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_BYTE: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_USHORT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_UINT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_ULONG: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_FLOAT: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_DOUBLE: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						case PH_RESV_KW_DECIMAL: varCmd = PH_REG + varTypeBit + ptrSymbol; break;
						default: break;
					}
					// Create address assignment codes for this member variable
					string asmAddressAssignmentCode = "";
					if(isConvertToPointerVar) {
						//asmAddressAssignmentCode = ";" + PH_VAR + bitSize.ToString() + " " + splitCode[1] + ";" + PH_POP;
						asmAddressAssignmentCode = ";" + PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + " " + splitCode[1] + ";" +
							PH_MOV + " " + className + ";" + PH_ADD + " " + ((previousAddressByteSize == 0) ? "" : previousAddressByteSize);
					}
					return varCmd + splitCode[1] + asmAddressAssignmentCode;
				}
				return code;
			}



			/// <summary>
			/// Convert class member variables to pointer declarations/pointer vars
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] PrepareClassStructure(string[] codes) {
				// Add empty string code array at the beginning of codes so we can insert any extra var declaration & initiation codes
				List<string> list = codes.ToList<string>();
				list.Insert(0, "");
				codes = list.ToArray<string>();

				for(int i = 0; i < codes.Length; i++) {
					// Replace static class to labels
					if(codes[i].IndexOf(PH_RESV_KW_STATIC_CLASS, 0) != -1) {
						codes = ReplaceClassToLabelAndConvertMemberVarToASM(codes, i, true);
						continue;
					}
					// Remove class and scope brackets
					if(codes[i].IndexOf(PH_RESV_KW_CLASS, 0) != -1) {
						codes = ReplaceClassToLabelAndConvertMemberVarToASM(codes, i, false);
						continue;
					}
				}
				string newCodes = "";
				for(int i = 0; i < codes.Length; i++) newCodes += codes[i];
				string[] newCodesArray = splitCodesToArray(newCodes);
				return newCodesArray;
			}

			/// <summary>
			/// Replace class definitions (may be normal class or static class, which is defined in classDefinitionString)
			/// And also converts class members to ASM code
			/// NOTE: Since normal class's member variables will become pointers, set isStaticClass to false
			/// Static class member variables will not be pointers, so set isStaticClass to true
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startCodeIndex"></param>
			/// <param name="classDefinitionString"></param>
			/// <param name="isConvertToPointerVar"></param>
			/// <returns></returns>
			private string[] ReplaceClassToLabelAndConvertMemberVarToASM(string[] codes, int startCodeIndex, bool isStaticClass) {
				string classDefinitionString = isStaticClass ? PH_RESV_KW_STATIC_CLASS : PH_RESV_KW_CLASS;
				// Remove class and scope brackets
				int endScopeBracket = FindEndScopeBlock(codes, startCodeIndex, "{", "}");
				string className = codes[startCodeIndex].Replace(PH_RESV_KW_CLASS, "");

				for(int i = startCodeIndex; i < endScopeBracket; i++) {
					if(codes[i] == "{") {
						startCodeIndex = i + 1;
						break;
					}
				}

				// If not static class, we need to add a pointer var declaration and an address assignment code (from stack via POP) to the top that stores the address of the caller instance variable
				if(!isStaticClass) {
					List<string> codeList = codes.ToList<string>();
					codeList.Insert(startCodeIndex, PH_REG + "s" + TARGET_ARCH_BIT_SIZE + " " + PH_ID + PH_POINTER + PH_ID + className + ";" +
						PH_POP + " " + className + ";");

					List<int> list = new List<int>();
					list.Add(TARGET_ARCH_BIT_SIZE / 8);
					ActualClassMemberVarSizeList.Add(className, list);

					codes = codeList.ToArray<string>();
				}


				//codes[startCodeIndex] = PH_NOP + " " + ReplaceFirstOccurance(codes[startCodeIndex], classDefinitionString, "") + ";";
				//codes[startCodeIndex + 1] = "";
				//codes[endScopeBracket] = "";

				// Stores the total number of byte size this class has
				int totalAddressByteSize = 0;

				// Now convert all class member variables to pointer variables
				for(int i = startCodeIndex; i < endScopeBracket; i++) {
					// If dealing with normal class, all member variables are already placed at the top of the class block
					// So, once we reach a function declaration, we ignore the rest
					// (Note that class member var initiation codes are already placed inside the constructor, so no need to consider these initiations)

					string[] splitFuncName = codes[i].Split(' ');
					if(AllFunctionKeywords.Contains<string>(splitFuncName[0] + " ")) {
						//if(codes[i].IndexOf(PH_RESV_KW_FUNCTION, 0) != -1) {
						if(!isStaticClass) {
							// If not static class, then we need to put a RET code to return to the caller, which we will return the address of the newely created class instance
							codes[i - 1] += PH_PSH + " " + className + ";" + PH_RET + ";";
							break;
						}
						// If static class, ignore function scope block
						int endOfFunctionBlock = FindEndScopeBlock(codes, i, "{", "}");
						i = endOfFunctionBlock;
						continue;
					}
					string[] splitCode = codes[i].Split(' ');
					// If variable declaration found, convert to ASM code (members should be pointer vars)
					if(ClassMemberReservedKeyWords.Contains<string>(splitCode[0] + " ")) {
						// Now, if we are dealing with a static class, all class member variables are static
						// Meaning, that any non-function member codes (i.e. static variable declaration and initiation) must be called at the very beginning of program execution
						// Therefore, we need to move the entire static variable declaration and initiation to the top of our program code
						if(isStaticClass) {
							string varDeclareCode = ConvertVarDeclarationsToASM(codes[i], 0, !isStaticClass, className) + ";";
							codes[i] = "";
							codes[i + 1] = "";
							i++; // Skip semicolon code index
									 // Now, check if we have initiation code for this variable straight after
							for(int j = i; j < endScopeBracket; j++) {
								// If function found, break
								splitFuncName = codes[j].Split(' ');
								if(AllFunctionKeywords.Contains<string>(splitFuncName[0] + " ")) {

									//if(codes[j].IndexOf(PH_RESV_KW_FUNCTION, 0) != -1) {
									break;
								} else if(codes[j] == splitCode[1]) {
									// If yes, search until end of statement found
									for(int k = j; k < endScopeBracket; k++) {
										if(codes[k] == ";") {
											string initCode = "";
											for(int m = i; m <= k; m++) {
												initCode += codes[m];
												codes[m] = "";
											}
											// Move the initiation code after the declaration
											varDeclareCode += initCode;
											i = k; // Skip semicolon code index
											break;
										}
									}
									break;
								}
							}
							codes[0] += varDeclareCode;
						} else {
							// Get previous var's address size
							for(int j = i; j >= startCodeIndex; j--) {
								string varDeclareCode = Regex.Match(codes[j], PH_REG + "(.*?) ").Value.Trim();
								if(varDeclareCode.IndexOf(PH_REG, 0) != -1) {

									List<int> list = ActualClassMemberVarSizeList[className];
									int byteSize = list[list.Count - 1];
									totalAddressByteSize += byteSize;
									/*
									try {
										int byteSize = Convert.ToInt32(varDeclareCode.Replace(PH_REG, "").Replace("u", "").Replace("s", ""));
										byteSize = byteSize / 8;
										totalAddressByteSize += byteSize;
									} catch { }
									*/

									break;
								}
							}
							// We are dealing with normal class, so just convert the declaration to ASM and replace it with the original code at the same position in the line of code
							codes[i] = ConvertVarDeclarationsToASM(codes[i], totalAddressByteSize, !isStaticClass, className);
							// Store the actual member types and size
							if(ActualClassMemberVarSizeList.ContainsKey(className)) {
								List<int> list = ActualClassMemberVarSizeList[className];
								string[] splitMemberVarCode = codes[i].Split(' ');
								int bitSize = Convert.ToInt32(splitMemberVarCode[0].Replace(PH_REG, "").Replace("u", "").Replace("s", ""));
								list.Add(bitSize / 8);
								ActualClassMemberVarSizeList[className] = list;
							}
							/*
							if(ActualClassMemberVarSizeList.ContainsKey(className)) {
								List<string> list = ActualClassMemberVarSizeList[className];
								list.Add(codes[i]);
								ActualClassMemberVarSizeList[className] = list;
							} else {
								List<string> list = new List<string>();
								list.Add(codes[i]);
								ActualClassMemberVarSizeList.Add(className, list);
							}*/
							// Actually, all non-static class member variables will be pointers, so we need to treat them as address pointers (meaning they should have a size of maximum bytes allowed)
							codes[i] = Regex.Replace(codes[i], PH_REG + @"(.*?)\d ", PH_REG + "s" + TARGET_ARCH_BIT_SIZE + " ");
							codes[i] = Regex.Replace(codes[i], PH_VAR + @"(.*?)\d ", PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + " ");

							i++; // Skip semicolon code index
									 // For each class members declared inside normal classes, add a pointer placeholder (𒀭PH_PTR𒀭) so we know that we are operating on pointer vars
							for(int j = i; j < endScopeBracket; j++) {
								if(codes[j] == splitCode[1]) {
									codes[j] = PH_ID + PH_POINTER + PH_ID + codes[j];
								}
							}
						}
					}
				}
				return codes;
			}

			/// <summary>
			/// Remove namespaces
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] RemoveNamespaces(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					// Remove namespace and scope brackets
					if(codes[i].IndexOf(PH_RESV_KW_NAMESPACE, 0) != -1) {
						int endScopeBracket = FindEndScopeBlock(codes, i, "{", "}");
						codes[i] = "";
						codes[i + 1] = "";
						codes[endScopeBracket] = "";
					}
				}
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			#endregion

			#region Resolve Math Expressions

			/// <summary>
			/// Convert expressions so that we can convert the expression into Reverse Polish Notation (RPN)
			/// 
			/// e.g.
			/// namespace N {
			///		class Exp {
			///			public Exp() {
			///				int a = 1;
			///				int result = (add(add(1, 2), 3) * (a - 5));  <---- Convert this to RPN
			///			}
			///			public int add(int a, int b) {
			///				return a + b;
			///			}
			///		}
			/// }
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string BreakDownExpression(string expression, string[] codes) {
				// Replace sequential function call scope separator "." with placeholder "#", to distinguish between variables and function calls
				string seqFuncCallPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
				expression = expression.Replace(").", ")#");
				// Replace variable scope separator "." with placeholder, as symbols within variables are not allowed when parsing to RPN
				string scopeSepPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
				expression = expression.Replace(".", scopeSepPH);

				// Remove all empty spaces in the expression
				expression = expression.Replace(" ", "");

				string incrementPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
				string decrementPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

				// Convert increment/decrement to placeholders
				if(expression.IndexOf("＋") != -1 && expression.IndexOf("—") != -1) {
					string[] splitExp = Regex.Split(expression, @"(?<=\+)");
					// NOTE: int d = b+++a + 2; // This will be determined in two ways: b++ + a + 2, or b + ++a + 2 (C# interprets as the first one)
					// However, int d = ++a + ++a + --f + f--;
					expression = expression.Replace("＋", incrementPH);
					expression = expression.Replace("—", decrementPH);
				}

				// Break down expressions containing arrays and functions into smaller expressions
				List<string> results = BreakDownArrayFunc(expression);

				// Remove redundant code and optimize it
				results = EliminateRedundantExpressions(results);

				// Exceptional condition: If we have a closing bracket, and alphanumeric values ([a-zA-Z0-9]) straight after the closing brackets, it indicates that an array's function is called
				// Therefore, in order to prevent this expression being broken and referencing a temporary variable that is not instantiated with anything, we will need to create a new instance for
				// e.g. NirmnpycqProgramirmnpycqMainirmnpycqi[0]irmnpycqsetA(10)   ----->   This is "N.Program.Main.i[0].setA(10);"
				// However, if we run this ResolveExpressionWithBrackets method, we will end up with:
				// -------------
				// var esgyfugp=0;
				// var result_esgyfugp=NsxjplyupProgramsxjplyupMainsxjplyupi[esgyfugp];
				// fbrpvhhh=result_esgyfugpsxjplyupsetA(10);			
				// -------------
				// Where result_esgyfugp becomes a reference, but is not instantiated, so we need to instantiate result_esgyfugp first in order to properly assign the reference to it

				if(Regex.IsMatch(expression, @"\][a-zA-Z0-9]")) {
					for(int i = 0; i < results.Count; i++) {
						string variable = Regex.Match(results[i], @"(?<=" + PH_RESV_KW_VAR + @").+?(?==)").Value;
						if(variable == "") continue;
						//string rightExp = Regex.Match(results[i], @"(?<==).+?(?=;)").Value;
						string rightExp = Regex.Match(results[i], @"(?<==).+?(?=\[)").Value;
						for(int j = 0; j < results.Count; j++) {
							string chkExp = Regex.Match(results[j], @"=.+?(?=;)").Value;
							if(chkExp.IndexOf("=" + variable + scopeSepPH) != -1) {
								//results[j] = results[j].Replace("=" + variable, "=" + rightExp);
								// Remove the "var " keyword from the result_esgyfugp variable
								results[i] = results[i].Substring(PH_RESV_KW_VAR.Length);
								// Add a result_esgyfugp instantiation code at the top of our list (i.e. INT32 result_esgyfugp = new INT32();)
								string[] splitNamespaceCode = rightExp.Split(new string[] { scopeSepPH }, StringSplitOptions.None);
								string instanceObjName = "";

								for(int k = 0; k < splitNamespaceCode.Length; k++) instanceObjName += splitNamespaceCode[k] + ".";
								instanceObjName = instanceObjName.Substring(0, instanceObjName.Length - 1);

								// Instance object name could be instantiated either in the following forms (normalized code string, or code string not normalized yet):
								// [0] ptr𒀭 N.Program.Main.i;var nopxbrsm=2;N.Program.Main.i=𒀭PH_NEW𒀭N.INT32[nopxbrsm];
								// ------ OR ------
								// [1] ptr𒀭 N.Program.Main.i
								// [2] =
								// [3] 𒀭PH_NEW𒀭N.INT32[2]
								// [4] ;
								// ------ OR ------
								// [1] ptr𒀭 N.Program.Main.i
								// [2] ;
								// [3] N.Program.Main.i
								// [4] =
								// [5] 𒀭PH_NEW𒀭N.INT32[2]
								// [6] ;

								string className = "";
								// So, first of all we search the entire code and see if we can find {instanceObjName}=𒀭PH_NEW𒀭
								for(int k = 0; k < codes.Length; k++) {
									if(codes[k].IndexOf(instanceObjName) == -1) continue;

									// Search for 𒀭PH_NEW𒀭 until we reach a semicolon in any of the code of line..
									for(int m = k; m < codes.Length; m++) {
										if(codes[m].IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
											// Found, so get the class name between "𒀭PH_NEW𒀭" and "[" from "𒀭PH_NEW𒀭N.INT32["
											className = Regex.Match(codes[m], @"(?<=" + PH_ID + PH_NEW + PH_ID + @").+?(?=\[)").Value;
											break;
										}
										if(codes[m].IndexOf(";") != -1) {
											break;
										}
									}
									// If we haven't found any 𒀭PH_NEW𒀭 keyword, then search again
									if(className != "") {
										// Found and got the class name!
										break;
									}
								}
								// Replace dot separators with placeholders
								className = className.Replace(".", scopeSepPH);
								//results[i] = className + " " + results[i];
								//results.Insert(0, variable + "=" + PH_ID + PH_NEW + PH_ID + className + "();");
								//results.Insert(0, PH_RESV_KW_PTR + variable + ";");
								//results.Insert(0, variable + "=" + PH_ID + PH_NEW + PH_ID + className + "();");
								results.Insert(0, className + " " + variable + ";");
								break;
							}
						}
					}
				}

				// Resolve comma separators
				results = BreakDownCommaSeparators(results);

				// Resolve sequential function calls (functions that contain a period "." after it self)
				// e.g. str=add(2,3).sub(1,1).ToString();
				// Note that sequential function call separators are changed to "#" symbol during expression breakdown phase
				// e.g. str=result_pxl#result_eip#result_ran;
				results = BreakDownFunctionCallSequence(results);


				// Now that all arrays, function calls, sequential function calls are broken down, we can convert the math operations into RPN format
				// Using FunctionZero for converting to RPN notation: https://github.com/Keflon/FunctionZero.ExpressionParserZero
				results = ConvertExpressionToRPN(results);





				// Now, resolve new keyword instantiation line that contains "var " and replace the "var " with the actual class name type
				for(int i = 0; i < results.Count; i++) {
					string variable = Regex.Match(results[i], @"(?<=" + PH_RESV_KW_VAR + @").+?(?==)").Value;
					if(variable == "") continue;
					if(results[i].IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
						// Get the class name
						//string className = Regex.Match(results[i], @"(?<=" + PH_ID + PH_NEW + PH_ID + @").+?(?=\()").Value;
						results.Insert(i, PH_RESV_KW_PTR + variable + ";");
						results[i + 1] = results[i + 1].Replace(PH_RESV_KW_VAR, "");
					}
				}


				string finalResults = string.Join("", results);


				// Replace placeholder back to its original values
				finalResults = finalResults.Replace(scopeSepPH, ".");

				// Convert back increment/decrement to placeholders
				finalResults = finalResults.Replace(incrementPH, "＋");
				finalResults = finalResults.Replace(decrementPH, "—");

				return finalResults;
			}

			/// <summary>
			/// Converts simple math operations to RPN format
			/// </summary>
			/// <param name="expressions"></param>
			/// <returns></returns>
			private List<string> ConvertExpressionToRPN(List<string> expressions) {
				ExpressionParser ep = new ExpressionParser();

				/*
				- 	12 	UnaryMinus
				+ 	12 	UnaryPlus
				! 	12 	UnaryNot
				~ 	12 	BitwiseUnaryComplement
				* 	11 	Multiply
				/ 	11 	Divide
				% 	11 	Modulo
				+ 	10 	Add
				- 	10 	Subtract

				<<	9 Bitwise shift left
				>>	9 Bitwise shift right

				< 	9 	LessThan
				> 	9 	GreaterThan
				>= 	9 	GreaterThanOrEqual
				<= 	9 	LessThanOrEqual
				!= 	8 	NotEqual
				== 	8 	Equality
				& 	7 	BitwiseAnd
				^ 	6 	BitwiseXor
				| 	5 	BitwiseOr
				&& 	4 	LogicalAnd
				|| 	3 	LogicalOr
				= 	2 	SetEquals
				, 	1 	Comma
				*/

				for(int i = 0; i < expressions.Count; i++) {
					// Ignore function calls
					if(Regex.IsMatch(expressions[i], @"[a-zA-Z0-9_" + PH_ID + @"]\(")) continue;
					// Ignore arrays
					if(expressions[i].IndexOf("[") != -1) continue;
					// Ignore expressions not containing any math/expression/logical operators
					if(!Regex.IsMatch(expressions[i], @"([*\^\/\+\-%&\|\^!~<>])")) {
						if(expressions[i].IndexOf("==") == -1) continue;
					}
					string operandLeftExpression = Regex.Match(expressions[i], @"(.*?)=").Value;
					string operandRightExpression = expressions[i].Substring(operandLeftExpression.Length).Replace(";", "");

					// Create a new variable to store increment/decrement placeholders
					string tmpVarInc = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
					// Create a new variable to store increment/decrement placeholders
					string tmpVarDec = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

					operandRightExpression = operandRightExpression.Replace("＋", tmpVarInc);
					operandRightExpression = operandRightExpression.Replace("—", tmpVarDec);

					TokenList compiledExpression = ep.Parse(operandRightExpression);
					//var compiledExpression = ep.Parse(operandRightExpression);

					string rpnStr = "\"";
					foreach(var t in compiledExpression) {
						rpnStr += t.ToString() + "#";
					}
					rpnStr = rpnStr.Substring(0, rpnStr.Length - 1) + "\";";
					expressions[i] = operandLeftExpression + rpnStr;

					expressions[i] = expressions[i].Replace(tmpVarInc, "＋");
					expressions[i] = expressions[i].Replace(tmpVarDec, "—");


					/*
					// Revert back temporary strings to string placeholders
					foreach(var kvp in stringPHList) {
						expressions[i] = expressions[i].Replace(kvp.Key, kvp.Value);
					}
					// Revert back temporary char to char placeholders
					foreach(var kvp in charPHList) {
						expressions[i] = expressions[i].Replace(kvp.Key, kvp.Value);
					}*/
				}

				return expressions;
			}


			/*
			/// <summary>
			/// Converts simple math operations to RPN format
			/// </summary>
			/// <param name="expressions"></param>
			/// <returns></returns>
			private List<string> ConvertExpressionToRPN(List<string> expressions) {
				ExpressionParser ep = new ExpressionParser();
				for(int i = 0; i < expressions.Count; i++) {
					// Ignore function calls
					if(Regex.IsMatch(expressions[i], @"[a-zA-Z0-9_" + PH_ID + @"]\(")) continue;
					// Ignore arrays
					if(expressions[i].IndexOf("[") != -1) continue;
					// Ignore expressions not containing any math/expression/logical operators
					if(!Regex.IsMatch(expressions[i], @"([*\^\/\+\-%&\|\^!~<>])")) {
						if(expressions[i].IndexOf("==") == -1) continue;
					}
					string operandLeftExpression = Regex.Match(expressions[i], @"(.*?)=").Value;
					string operandRightExpression = expressions[i].Substring(operandLeftExpression.Length).Replace(";", "");
					TokenList compiledExpression = ep.Parse(operandRightExpression);
					string rpnStr = "\"";
					foreach(var t in compiledExpression) {
						rpnStr += t.ToString() + "#";
					}
					rpnStr = rpnStr.Substring(0, rpnStr.Length - 1) + "\";";
					expressions[i] = operandLeftExpression + rpnStr;
				}
				return expressions;
			}
			*/


			/// <summary>
			/// Break down expressions containing arrays, functions into smaller expression
			/// </summary>
			/// <param name="expressions"></param>
			/// <returns></returns>
			private List<string> BreakDownArrayFunc(string expression) {
				List<string> combinedResults = new List<string>();
				// If we have any arrays inside the expression, resolve it first
				List<string> resultsArrayFuncResolved = new List<string>();
				if(expression.IndexOf("[") != -1) {
					// Resolve arrays in expression (e.g. "N.a[N.b+1+c[6/add(2,3)]]=1+N.add(N.a[2],3)")
					resultsArrayFuncResolved = BreakDownExpressionWithBrackets(expression, "[", "]");
					// Remove redundant code and optimize it
					resultsArrayFuncResolved = EliminateRedundantExpressions(resultsArrayFuncResolved);

					// Next, resolve functions (functions contain round brackets)
					foreach(string s in resultsArrayFuncResolved) {
						if(s.IndexOf(PH_RESV_KW_VAR) == 0) {
							// Are there any function calls?
							int endFuncCallIndex = s.IndexOf(")");
							if(endFuncCallIndex != -1) {
								string resolveExpTarget = s.Substring(PH_RESV_KW_VAR.Length).Replace(";", "");
								List<string> funcResResults = BreakDownExpressionWithBrackets(resolveExpTarget, "(", ")");
								funcResResults[funcResResults.Count - 1] = PH_RESV_KW_VAR + funcResResults[funcResResults.Count - 1];
								combinedResults.AddRange(funcResResults);
							} else {
								combinedResults.Add(s);
							}
						} else {
							int endFuncCallIndex = s.IndexOf(")");
							if(endFuncCallIndex != -1) {
								string resolveExpTarget = s.Replace(";", "");
								List<string> funcResResults = BreakDownExpressionWithBrackets(resolveExpTarget, "(", ")");
								// Remove redundant code and optimize it
								funcResResults = EliminateRedundantExpressions(funcResResults);
								combinedResults.AddRange(funcResResults);
							} else {
								combinedResults.Add(s);
							}
						}
					}
				} else {
					// Resolve function calls in expression (e.g. "c = add(2,sub(3,4))")
					combinedResults = BreakDownExpressionWithBrackets(expression, "(", ")");
				}

				return combinedResults;
			}

			/// <summary>
			/// Break down / Re-arranges sequential function calls
			/// e.g.
			/// var a = add(www,ggg);  <----  Need to remove
			/// var b = ToString();    <----  Need to remove
			/// var c = Replace(xxx,yyy);    <----  Need to remove
			/// var final_results = a#b#c;    <---- Need to replace with add(www,ggg)#ToString()#Replace(xxx,yyy);		
			/// </summary>
			/// <param name="expressions"></param>
			/// <returns></returns>
			private List<string> BreakDownFunctionCallSequence(List<string> expressions) {
				for(int i = 0; i < expressions.Count; i++) {
					string s = expressions[i];
					// Now, inside the entire expression list, we need to remove the line of code that is trying to store the concatenated function calls as variables
					// e.g.
					// var a = add(www,ggg);  <----  Need to remove
					// var b = ToString();    <----  Need to remove
					// var c = Replace(xxx,yyy);    <----  Need to remove
					// var final_results = a#b#c;    <---- Need to replace with add(www,ggg)#ToString()#Replace(xxx,yyy)
					// And, also the expression on the right side of the assignemnt operand needs to be replaced with the actual sequential function call
					// If a sequential function call contains other operators (such as +, -, /, *) then we need to separate the function call results with the other values
					// e.g.
					// str=str+add(snd0,snd1)#ToString()#Replace(ebk0,ebk1)*result_fjg;
					// --->
					// var seqfn_call=add(snd0,snd1)#ToString()#Replace(ebk0,ebk1);
					// str=str+seqfn_call*result_fjg;
					if(s.IndexOf("#") != -1) {
						string operandLeftExpression = Regex.Match(s, @"(.*?)=").Value;
						string operandRightExpression = s.Substring(operandLeftExpression.Length);
						string[] funcCalls = Regex.Match(operandRightExpression, @"(?<=[^a-zA-Z_" + PH_ID + @"])(.*?)(?=[^a-zA-Z0-9#_" + PH_ID + @"])").Value.Split('#');
						string finalRightExpression = s;
						List<string> newExpressionList = new List<string>();
						string functionCallSequenceStr = "";

						for(int j = 0; j < funcCalls.Length; j++) {
							//string funcCallExpression = "";
							// For each expressions in our list, search for the variable declaration for the function call up to but not including the current line of expression
							for(int k = 0; k < i; k++) {
								string varDeclarationStr = Regex.Match(expressions[k], @"(?<=" + PH_RESV_KW_VAR + ").+?(?==)").Value;
								string actualFuncCall = Regex.Match(expressions[k], @"(?<==).+?(?=;)").Value;
								if(varDeclarationStr == funcCalls[j]) {
									finalRightExpression = finalRightExpression.Replace(funcCalls[j], actualFuncCall);
									functionCallSequenceStr += actualFuncCall + ((j < funcCalls.Length - 1) ? "#" : "");
									// This function call becomes invalid
									expressions[k] = "";
									break;
								}
							}
						}
						expressions[i] = finalRightExpression;

						// If a sequential function call contains other operators (such as +, -, /, *) then we need to separate the function call results with the other values
						// e.g.
						// str=str+add(snd0,snd1)#ToString()#Replace(ebk0,ebk1)*result_fjg;
						// --->
						// var seqfn_call=add(snd0,snd1)#ToString()#Replace(ebk0,ebk1);
						if(Regex.IsMatch(finalRightExpression, @"([*()\^\/\+\-%&\|\^!~])")) {
							// Create a new variable to store the function call sequence results and insert into the expression list
							string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
							expressions[i] = expressions[i].Replace(functionCallSequenceStr, tmpVar);
							expressions.Insert(i, PH_RESV_KW_VAR + tmpVar + "=" + functionCallSequenceStr + ";");
							i++;
						}
					}
				}
				// Remove any blank array list items
				expressions = expressions.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

				return expressions;
			}

			/// <summary>
			/// Remove redundant code and optimize it:
			/// e.g. Following code sequence should be converted to just a single expression:
			/// var final_aaa=result_bbb;
			/// var final_ccc=final_aaa;
			/// --- Shoud be converted to: ---
			/// var final_ccc=result_bbb;
			///
			/// Also, codes containing "unused" variables must be eliminated to avoid floating expressions
			/// e.g.
			/// var aaa=234;    <----   not used in the code list
			/// var final_aaa=234;
			/// --- Shoud be converted to: ---
			/// var final_aaa=234;
			/// </summary>
			/// <param name="expressions"></param>
			/// <returns></returns>
			private List<string> EliminateRedundantExpressions(List<string> expressions) {
				// int key, as variable name that is declared with the "var " keyword (it is unique)
				// int value, as number of times in the list where the "variable was used as part of an assignment operation"
				Dictionary<string, int> declaredVarList = new Dictionary<string, int>();

				// For each expressions in our list, remove unrequired round brackets
				// e.g.
				// var xxx=(yyy);
				// --->
				// var xxx=yyy;
				for(int i = 0; i < expressions.Count; i++) {
					string s = expressions[i];
					// Check if there's only one round bracket pairs
					string testRight = Regex.Match(s, @"(?<==).+?(?=;)").Value;
					if((testRight.IndexOf("(") == 0) && (testRight.IndexOf(")") == testRight.Length - 1)) {
						string replaceWith = testRight.Substring(1, testRight.Length - 2);
						expressions[i] = Regex.Replace(s, @"(?<==).+?(?=;)", replaceWith);
					}
				}

				// For each expressions in our list, further analyze & optimize
				for(int i = 0; i < expressions.Count; i++) {
					string varName = Regex.Match(expressions[i], "(?<=" + PH_RESV_KW_VAR + ").+?(?==)").Value;
					if(i + 1 < expressions.Count) {
						string chkVarCurrent = expressions[i];
						string chkVarNext = expressions[i + 1];
						string testRight = Regex.Match(chkVarNext, "(?<==).+?(?=;)").Value;
						if(varName == testRight) {
							string optimizedExp = Regex.Match(chkVarNext, "(.*?)=").Value + Regex.Match(chkVarCurrent, "(?<==).+?;").Value;
							expressions[i] = "";
							expressions[i + 1] = optimizedExp;
						}
					}
					// Store all variable assignments in a list and check if it is used in latter expressions as we iterate through the list
					// Importantly, we need to add an empty varName if the expression does not exist, so we know in which expression list index this var was declared
					if(varName != "") declaredVarList.Add(varName, 0);
					// For each declared variables that we already have in our list, check if this current line of expression uses any of the variables
					// (just do a simple check, assuming the variable names do not overlap with other variable/function/object names)
					string expRight = Regex.Match(expressions[i], "(?<==).+?(?=;)").Value;
					// If the expression does not contain anything (var xxx=;) then remove this line
					// Also, we might have added a pointer on top of our line of code for assigning instantiated array objects, so we need to ignore any non-assignment line that contains "ptr𒀭 "
					if(expRight == "" && expressions[i].IndexOf(PH_RESV_KW_PTR) == -1) {
						expressions[i] = "";
					}
					for(int j = 0; j < declaredVarList.Count; j++) {
						var item = declaredVarList.ElementAt(j);
						// If the expression does not contain a variable declaration, we will need to check the entire expression (both left and right of assignemnt operator)
						if(expressions[i].IndexOf(PH_RESV_KW_VAR, 0) == -1) {
							if(expressions[i].IndexOf(item.Key) != -1) {
								declaredVarList[item.Key]++;
							}
						} else {
							// Check if variable exist in the current right side expression
							if(expRight.IndexOf(item.Key) != -1) {
								declaredVarList[item.Key]++;
							}
						}
					}
				}

				// Remove redundant variables that are never used in the expressions
				for(int j = 0; j < declaredVarList.Count; j++) {
					var item = declaredVarList.ElementAt(j);
					if(item.Value == 0) expressions[j] = "";
				}

				// Remove any blank array list items
				expressions = expressions.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
				return expressions;
			}



			private List<string> BreakDownExpressionWithBrackets(string expression, string openingBracket, string closingBracket) {
				List<string> results = new List<string>();

				// Before evaluating the expression, we need to see if the expression contains an assignment operator:
				// "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>="
				// If we do, then we will break down the expression on the left and right separately
				string[] breakUpExpression = BreakUpAssignmentExpression(expression);
				// We found some kind of assignment operator
				if(breakUpExpression[0] != null) {
					// Since ResolveExpressionWithBrackets method returns the final code in "results = xxxx;" format,
					// We will just need the "xxxx" part
					List<string> leftResults = ResolveExpressionWithBrackets(breakUpExpression[1], openingBracket, closingBracket, true);
					List<string> rightResults = ResolveExpressionWithBrackets(breakUpExpression[2], openingBracket, closingBracket);

					/*
					string leftResultsFinalExpression = leftResults[leftResults.Count - 1];
					string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true);
					leftResults.Insert(leftResults.Count - 1, "var " + tmpVar + "=" + leftResultsFinalExpression + ";");
					for(int i = 0; i < rightResults.Count; i++) {
						rightResults[i] = rightResults[i].Replace(PH_ID + PH_LEFTEXP_RESULT + PH_ID, tmpVar);
					}*/

					string rightResultFinalVar = Regex.Match(rightResults[rightResults.Count - 1], @"(?<=" + PH_RESV_KW_VAR + ").+?(?==)").Value;
					results.AddRange(rightResults);
					leftResults[leftResults.Count - 1] += breakUpExpression[0] + rightResultFinalVar + ";";
					results.AddRange(leftResults);

					//results = string.Join("", rightResults) + string.Join("", leftResults) + breakUpExpression[0] + rightResultFinalVar + ";";
				} else {
					results = ResolveExpressionWithBrackets(expression, openingBracket, closingBracket);

					// Resolve arrays inside expression
					//results = string.Join("", ResolveExpressionWithBrackets(expression, openingBracket, closingBracket));
				}

				return results;
			}

			/// <summary>
			/// Breaks up expression into arrays in the following format:
			/// return string array [0] = assignment operator (e.g. "=", "-=", "+=", etc)
			/// return string array [1] = expression on the left of the assignment operator
			/// return string array [2] = expression on the right of the assignment operator
			/// </summary>
			/// <param name="expression">Any expression that may or may not contain assignment operators</param>
			/// <returns>Expressions split up as string array, with the first array index being the assignment operator in string format symbol</returns>
			private string[] BreakUpAssignmentExpression(string expression) {
				string[] retStr = new string[3];
				string[] ignoreOpearatorList = new string[] { "<=", ">=", "==" };
				string[] assignmentOperatorList = new string[] { "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=" };
				// Replace logical operators containing equal sign with placeholder
				for(int i = 0; i < ignoreOpearatorList.Length; i++) {
					expression = expression.Replace(ignoreOpearatorList[i], PH_ID + i);
				}
				// Check if we have an assignment operator of any kind
				for(int i = 0; i < assignmentOperatorList.Length; i++) {
					int assignmentOperatorIndex = expression.IndexOf(assignmentOperatorList[i]);
					if(assignmentOperatorIndex != -1) {
						// Found assignemnt operator
						retStr[0] = assignmentOperatorList[i];
						retStr[1] = expression.Substring(0, assignmentOperatorIndex);
						retStr[2] = expression.Substring(assignmentOperatorIndex + retStr[0].Length);
						// Covert assignment operator to a basic assignment operator (=)
						// e.g. x *= y - z  --->  x = x * (y - z)
						int equalSignIndex = retStr[0].IndexOf("=");
						if(equalSignIndex != 0) {
							//retStr[2] = PH_ID + PH_LEFTEXP_RESULT + PH_ID + retStr[0].Substring(0, equalSignIndex) + "(" + retStr[2] + ")";
							retStr[2] = retStr[1] + retStr[0].Substring(0, equalSignIndex) + "(" + retStr[2] + ")";
							retStr[0] = "=";
						}
						break;
					}
				}
				if(retStr[0] == null) return retStr;
				// Convert placeholder back to logical operators
				for(int j = 0; j <= 2; j++) {
					for(int i = 0; i < ignoreOpearatorList.Length; i++) retStr[j] = retStr[j].Replace(PH_ID + i, ignoreOpearatorList[i]);
				}
				return retStr;
			}

			private List<string> ResolveExpressionWithBrackets(string expression, string openingBracket, string closingBracket, bool isFinalStatementFloating = false) {
				List<string> resultList = new List<string>();
				if(expression == null) return resultList;
				for(int i = 0; i < expression.Length; i++) {
					if(expression[i] == Convert.ToChar(openingBracket)) continue;
					if(expression[i] == Convert.ToChar(closingBracket)) {
						int startFuncVarIndex = ObtainFuncVarStartIndex(expression, i, openingBracket);
						string fullArrayFuncStr = expression.Substring(startFuncVarIndex, i - startFuncVarIndex + 1);
						// Get array/function name (opening brackets are either "(", "[", "{", "<")
						string nameOfArrayFunc = Regex.Match(fullArrayFuncStr, @"^[^\(|\[\{<]+").Value;
						// Get contents inside the brackets
						string bracketContents = Regex.Match(fullArrayFuncStr, @"(?<=\" + openingBracket + @").+?(?=\" + closingBracket + ")").Value;
						// Generate random variable name to be used for assigning temporary value
						string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
						// Replace this expression with a placeholder
						string breakDownExp = PH_RESV_KW_VAR + tmpVar + "=" + bracketContents + ";";
						if(bracketContents != "") {
							resultList.Add(breakDownExp);
							breakDownExp = PH_RESV_KW_VAR + "result_" + tmpVar + "=" + nameOfArrayFunc + openingBracket + tmpVar + closingBracket + ";";
						} else {
							// If no parameters found withing brackets, just assign the function call to a result var
							breakDownExp = PH_RESV_KW_VAR + "result_" + tmpVar + "=" + nameOfArrayFunc + openingBracket + closingBracket + ";";
						}
						resultList.Add(breakDownExp);

						// Prepare new expression with old array/function replaced with the result expression
						expression = ReplaceFirstOccurance(expression, fullArrayFuncStr, "result_" + tmpVar);
						// Reset and start again the bracket search
						i = 0;
					}
				}

				if(resultList.Count == 0) {
					// Generate random variable name to be used for assigning temporary value
					string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
					// Replace this expression with a placeholder
					string breakDownExp = PH_RESV_KW_VAR + tmpVar + "=" + expression + ";";
					resultList.Add(breakDownExp);
				}

				if(isFinalStatementFloating) {
					// Since this method returns the final code in "results = xxxx;" format,
					// If this expression is the left equation of an assignment operand, we will just need the "xxxx" part
					// e.g.
					// var iyc=6/add(2,3);
					// var result_iyc = c[iyc];
					// var xjt = N.b + 1 + result_iyc;
					// var result_xjt = N.a[xjt];   ------>   We only need "N.a[xjt]" for this line
					string finalExpression = resultList[resultList.Count - 1];
					finalExpression = Regex.Replace(finalExpression, PH_RESV_KW_VAR + "(.*?)=", "").Replace(";", "");
					resultList[resultList.Count - 1] = finalExpression;
				} else {
					string breakDownExp = PH_RESV_KW_VAR + "final_" + RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier) + "=" + expression + ";";
					resultList.Add(breakDownExp);
				}

				return resultList;
			}

			/// <summary>
			/// If the expression contains a comma as separator, we will need to process them as separate variables from left to right in order
			/// e.g.
			/// var mgs=2,3;
			/// result_mgs = add(mgs);
			/// -------> CONVERTS TO ------>
			/// var xxx0=2;
			/// var xxx1=3;
			/// var xxx=add(xxx0, xxx1);
			/// result_mgs = xxx;
			/// </summary>
			/// <param name="expression"></param>
			/// <param name="openingBracket"></param>
			/// <param name="closingBracket"></param>
			/// <param name="isFinalStatementFloating"></param>
			/// <returns></returns>
			private List<string> BreakDownCommaSeparators(List<string> uncleanedList) {
				List<string> resultList = uncleanedList;
				for(int i = 0; i < resultList.Count; i++) {
					// If the bracket contents contains a comma as separator, we will need to process them from left to right in order
					if(resultList[i].IndexOf(",") != -1) {
						string tmpVarName = Regex.Match(resultList[i], "(?<=" + PH_RESV_KW_VAR + ").+?(?==)").Value;
						// If tmpVarName is empty (meaning that there are no "var " keyword for this expression), we ignore
						if(tmpVarName == "") continue;
						string[] splitParams = Regex.Match(resultList[i], "(?<==).+?(?=;)").Value.Split(',');
						string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
						string breakDownExp = "";
						string newBracketContents = "";
						List<string> tmpList = new List<string>();
						for(int j = 0; j < splitParams.Length; j++) {
							breakDownExp = PH_RESV_KW_VAR + tmpVar + j + "=" + splitParams[j] + ";";
							tmpList.Add(breakDownExp);
							newBracketContents += tmpVar + j + ((j == splitParams.Length - 1) ? "" : ",");
						}
						// Remove invalid code
						resultList.RemoveAt(i);
						// Get right side expression of next final assignment code (i.e. this will get either full function "add(mgs)" or full array "add[mgs]")
						//string rightExpStr = Regex.Match(resultList[i], "(?<==).+?(?=;)").Value;

						// Get function/array name
						//resultList[i] = resultList[i].Replace(rightExpStr, rightExpStr.Replace(tmpVarName, newBracketContents));
						resultList[i] = resultList[i].Replace(resultList[i], resultList[i].Replace("(" + tmpVarName + ")", "(" + newBracketContents + ")"));
						resultList.InsertRange(i, tmpList);
						i += tmpList.Count;
					}
				}
				return resultList;
			}

			/// <summary>
			/// Get the character index inside a string where the variable or function name starts
			/// e.g.
			/// 1+add(2,3)  -->  Given 9 as endOfScopeSymbolIndex returns 2
			/// a-xb[2] ---> Given 6 as endOfScopeSymbolIndex returns 2
			/// </summary>
			/// <param name="expression"></param>
			/// <param name="endOfScopeSymbolIndex"></param>
			/// <param name="openBracket">The bracket opening in string format: e.g. "(" or "["</param>
			/// <returns></returns>
			private int ObtainFuncVarStartIndex(string expression, int endOfScopeSymbolIndex, string openBracket, bool isIgnorePeriod = true) {
				int index = 0;
				string closeBracket = expression[endOfScopeSymbolIndex].ToString();
				int bracketCounter = 0;
				// Traceback until we find the beginning of scope (i.e. opening square bracket, opening round bracket)
				for(int j = endOfScopeSymbolIndex; j >= 0; j--) {
					if(expression[j] == Convert.ToChar(closeBracket)) {
						bracketCounter++;
						continue;
					}
					if(expression[j] == Convert.ToChar(openBracket)) {
						bracketCounter--;
						if(bracketCounter == 0) {
							// Get the array variable name by tracing back further (until we find a non alphabet or underscore char)
							for(int k = j - 1; k >= 0; k--) {
								bool isMatch = Regex.IsMatch(expression[k].ToString(), "[a-zA-Z0-9_" + (isIgnorePeriod ? "" : @"\.") + PH_ID + "]");
								if(!isMatch) {
									index = k + 1;
									break;
								}
							}
						}
					}
				}
				return index;
			}

			#endregion

			#region Preprocess

			/// <summary>
			/// This method generates line of codes that are tokenized, ready for converting into assembly code
			/// Remove line breaks, tabs, comments and comment blocks, resolves variable scope
			/// </summary>
			/// <param name="code">The entire program code string</param>
			/// <returns>Cleaned up code</returns>
			public string[] PreProcessCleanUp(string code) {
				// Do some preparations to code by sanitizing unwanted elements
				code = PreProcessCodeBeforeLOCSeparation(code);
				// Convert the program initialization entry point "[DScript_Init]" to a label so we can tell the Virtual Machine where the initialization block starts
				code = code.Replace(PH_RESV_KW_PROGRAM_INIT_POINT_ATTRIBUTE, PH_RESV_KW_PROGRAM_INIT_POINT_LABEL);
				// Convert the program main entry point "[DScript_Main]" to a label so we can tell the Virtual Machine where the main program starts
				code = code.Replace(PH_RESV_KW_PROGRAM_MAIN_LOOP_ATTRIBUTE, PH_RESV_KW_PROGRAM_MAIN_LOOP_LABEL);
				// Separate codes by tokens to make code of lines
				string[] codes = splitCodesToArray(code);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_SplitCodesToArray.txt", string.Join("\n", codes));
				ShowLog("- Adding missing scope...");
				// Add missing scope brackets (curly brackets) to "if" "else if" "while" "for"
				codes = AddMissingScopeBrackets(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_AddMissingScopeBrackets.txt", string.Join("\n", codes));
				ShowLog("- Resolving instantiation code...");
				// We need to convert all instantiation code (using the "new" keyword) to call this new constructor
				// So, we will search for all codes that contains the "new" keyword
				codes = ResolveInstantiationCode(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveInstantiationCode.txt", string.Join("\n", codes));
				// Convert all occurance of "string" to "char[]"
				codes = ConvertStringKeywordToCharArray(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ConvertStringKeywordToCharArray.txt", string.Join("\n", codes));
				ShowLog("- Converting pre-defined keyword to actual value...");
				// Replace pre-defined keywords (true, false, null, etc) with actual values (may be numeric, class objects, strings, etc)
				codes = ReplacePreDefinedKeywordsWithActualValue(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ReplacePreDefinedKeywordsWithActualValue.txt", string.Join("\n", codes));
				// Since DScript language does not support float/double internally,
				// if a floating number is found, we need to multiply it by 10^n to move the decimal point rightwards to make it a whole number
				codes = ResolveFloatDoubleSuffix(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveFloatDoubleSuffix.txt", string.Join("\n", codes));

				ShowLog("- Converting arrays to pointers...");
				// Convert arrays (including multi-dimentional arrays) declarations to pointers
				codes = ConvertArraysToCLangPointerNotations(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ConvertArraysToCLangPointerNotations.txt", string.Join("\n", codes));

				ShowLog("- Converting methods to functions...");
				// Convert C# methods to PHP-like functions
				codes = ConvertMethodToFunctions(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ConvertMethodToFunctions.txt", string.Join("\n", codes));

				ShowLog("- Resolving function returns...");
				// Add "return(0);" to the very bottom of functions (ALL functions must return somthing, even if it is a void function)
				codes = AddReturnToFunctions(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_AddReturnToFunctions.txt", string.Join("\n", codes));
				// For each "return(xxx);" statement, separate equation so we only have one variable inside the return bracket: e.g. "var ret = xxx; return(ret);"
				codes = ResolveReturnStatements(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveReturnStatements.txt", string.Join("\n", codes));
				ShowLog("- Resolving scope...");
				// Adds the specified parent scope definition to its specified child scope definition
				codes = ResolveScope(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope.txt", string.Join("\n", codes));

				ShowLog("- Preprocessing pointers...");
				// Get the list of variable types for pointers, and convert them to "ptr𒀭"
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(PH_RESV_KW_PTR) == -1) continue;
					string[] splitCode = codes[i].Split(' ');
					string varType = splitCode[0].Split('_')[0];
					ActualClassMemberPointerVarTypeList.Add(splitCode[1], varType);
					codes[i] = codes[i].Replace(splitCode[0] + " ", PH_RESV_KW_PTR);
				}

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreprocessPointers.txt", string.Join("\n", codes));

				ShowLog("- Converting labels to placeholders...");
				// Convert all string placeholders to actual char arrays
				//codes = ConvertStringPlaceHolderToCharArray(codes);
				// Convert labels "labelname:" to placeholder symbols, so we can distinguish between labels and class inheritences
				codes = ConvertLabelsToPlaceHolders(codes);

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_LabelsToPlaceholders.txt", string.Join("\n", codes));

				ShowLog("- Resolving extended classes...");
				// Resolve class inheritences
				codes = ResolveExtendedClass(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveExtendedClass.txt", string.Join("\n", codes));

				ShowLog("- Preprocessing static classes...");
				// Adds constructors to static class
				codes = AddConstructorsToStaticClass(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_AddConstructorsToStaticClass.txt", string.Join("\n", codes));
				ShowLog("- Preprocessing for loops...");
				// Preprocess for loop statements by converting them to "while" statements, and adding labels for start and end block of the entire for statement
				codes = PreProcessForBlock(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreProcessForBlock.txt", string.Join("\n", codes));
				ShowLog("- Converting while statements to if statements...");
				// Preprocess while loop statements by convertinf them to "if" statements, and adding labels for start and end block of the entire while statement
				codes = PreProcessWhileBlock(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreProcessWhileBlock.txt", string.Join("\n", codes));
				ShowLog("- Preprocessing if else blocks...");
				// Place the equation for logical comparison inside if/else if statements outside the if/else if brackets, and also place a label for the start and end block of the if statement
				codes = PreProcessIfElseBlock(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreProcessIfElseBlock.txt", string.Join("\n", codes));
				ShowLog("- Resolving floating function calls...");
				// Resolve floating function calls that usually returns some kind of value back to the caller
				codes = ResolveFloatingFunctionCalls(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveFloatingFunctionCalls.txt", string.Join("\n", codes));
				ShowLog("- Preprocessing chars to UTF16...");
				// Convert all char placeholders to actual decimal (UTF-16 / Decimal) representation
				codes = ConvertCharPlaceHolderToDecimal(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ConvertCharPlaceHolderToDecimal.txt", string.Join("\n", codes));
				ShowLog("- Preprocessing expressions...");
				// Preprocess / breakdown expressions
				codes = PreProcessFunctionParams(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreProcessFunctionParams.txt", string.Join("\n", codes));
				codes = PreProcessExpressions(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_PreProcessExpressions.txt", string.Join("\n", codes));
				ShowLog("- Resolving implicit var types...");
				// For all "var" keywords, determine their appropriate size if possible - if not able to determine, it will be converted to the most largest variable type (long)
				codes = ResolveImplicitVarType(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveImplicitVarType.txt", string.Join("\n", codes));
				ShowLog("- Restructuring class member vars...");
				// For each class, move all member variable initiation codes to the constructor, and variable assignments to top of class
				codes = RestructureClassMemberVars(codes, PH_RESV_KW_CLASS);
				// For each static class, move all member variable initiation codes to the constructor, and variable assignments to top of class
				codes = RestructureClassMemberVars(codes, PH_RESV_KW_STATIC_CLASS);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_RestructureClassMemberVars.txt", string.Join("\n", codes));
				ShowLog("- Deploying iasm codes...");
				// Deploy all manually coded IASM codes in the script
				codes = DeployManualIASM(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_DeployManualIASM.txt", string.Join("\n", codes));
				ShowLog("- Converting strings to char arrays...");
				// Convert all string placeholders to char arrays assigned with actual decimal (UTF-16 / Decimal) representation
				codes = ConvertStringPlaceHolderToCharDecimalArray(codes);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ConvertStringPlaceHolderToCharDecimalArray.txt", string.Join("\n", codes));
				ShowLog("- Resolving constructors...");
				// Move any variables that are declared as class members to constructor, if they are not being used in other places other than within the constructor
				codes = MoveTemporaryClassMemberVarToConstructor(codes, PH_RESV_KW_CLASS);
				// Move any variables that are declared as static class members to constructor, if they are not being used in other places other than within the constructor
				codes = MoveTemporaryClassMemberVarToConstructor(codes, PH_RESV_KW_STATIC_CLASS);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_MoveTemporaryClassMemberVarToConstructor.txt", string.Join("\n", codes));
				return codes;
			}

			/// <summary>
			/// Preprocesses the raw string of codes before separating it to lines of code
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string PreProcessCodeBeforeLOCSeparation(string code) {
				// Convert escape characters \" to placeholders
				code = code.Replace("\\\"", PH_ESC_DBL_QUOTE);
				code = code.Replace("'\"'", "'" + PH_ESC_DBL_QUOTE + "'");
				// Convert single quote within a char '\'' to placeholder
				code = code.Replace("\\\'", PH_ESC_SNGL_QUOTE);
				// Remove line breaks, tabs, comments and comment blocks
				code = CleanupCode(code);
				// Convert string literals to placeholders
				code = ConvertLiteralsToPlaceholders(code);
				// Convert increment/decrement operators to placeholders
				code = ConvertIncDecOperatorsToPlaceholders(code);
				// Remove multiple spaces to single space
				code = ReplaceSequentialRepStringToSingle(code, " ");

				// Resolve enum keywords and conver them to plain const int values
				code = ResolveEnum(code);
				// Generate new constructors to allow parameters
				code = ResolveConstructors(code);

				// Remove obsolete keywords that are only used for compatibility reasons with other languages
				code = ReplaceKeywords(code, ObsoleteKeywords);
				// Apply further optimizations for pre-processing code seaparations
				code = SanitizeKeywordForSeparation(code);
				// Convert "new" keywords to "new_"
				code = ConvertNewKeywordToPlaceholders(code);

				return code;
			}

			/// <summary>
			/// we need to convert all instantiation code (using the "new" keyword) to call this new constructor
			/// So, we will search for all codes that contains the "new" keyword
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveInstantiationCode(string[] codes) {
				// Next, we need to convert all instantiation code (using the "new" keyword) to call this new constructor
				// So, we will search for all codes that contains the "new" keyword
				List<string> codeList = codes.ToList<string>();
				string newKeyword = PH_ID + PH_NEW + PH_ID;
				for(int i = 0; i < codeList.Count; i++) {
					if(codeList[i].IndexOf(newKeyword) != -1) {
						// If className contains brackets, it means it's creating an array, so we ignore it
						if(codeList[i].IndexOf("[") != -1) continue;
						if(codeList[i - 1] != "=") continue;
						string[] classAndInstanceObj = codeList[i - 2].Split(' ');
						string className;
						string instanceObjName;
						if(classAndInstanceObj.Length == 1) {
							className = codeList[i].Replace(newKeyword, "");
							instanceObjName = codeList[i - 2];
						} else {
							className = classAndInstanceObj[0];
							instanceObjName = classAndInstanceObj[1];
						}
						codeList.Insert(i + 1, instanceObjName + "." + className + "_Constructor");
						codeList.Insert(i + 1, ";");
						codeList.Insert(i + 1, ")");
						codeList.Insert(i + 1, "(");
					}
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Since we do not support parameters for constructors, to support them,
			/// we need to generate a constructor without parameters and call a specifically generated constructor method from the constructor
			/// i.e.
			/// class A { public A(int a, int b) { ... } }
			/// -->
			/// class A { public A() {} public A_Constructor(int a, int b) { ... }; } }
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string ResolveConstructors(string code) {
				// Search for all classes and get the constructors & convert them to new constructors (if any constructors exist at all)
				MatchCollection mcClassMatches = Regex.Matches(code, PH_RESV_KW_CLASS + "(.*?){");
				foreach(Match m in mcClassMatches) {
					string className = m.Value.Replace(PH_RESV_KW_CLASS, "").Replace("{", "").Trim();
					// We might have class extends, so remove it
					className = Regex.Replace(className, ":(.*?)$", "").Trim();
					string constructor = Regex.Match(code, "public " + className + @"\((.*?)\)").Value;
					if(constructor == "") continue;
					string newConstructorCode = "public " + className + "(){} " + constructor.Replace("public " + className, "public void " + className + "_Constructor");
					code = code.Replace(constructor, newConstructorCode);
					// Now, we should have something like:
					// BEFORE:
					// public DString(){ CreateString(str); }
					// ---->
					// AFTER:
					// public DString(){} public void DString_Constructor(string str) { CreateString(str); }
				}
				return code;
			}

			/// <summary>enum resolver</summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string ResolveEnum(string code) {
				MatchCollection mc = Regex.Matches(code, " " + PH_RESV_KW_ENUM + "(.*?)}", RegexOptions.Singleline);
				foreach(Match m in mc) {
					int enumCounter = 0;
					string newCode = m.Value.Replace(" " + PH_RESV_KW_ENUM, " " + PH_RESV_KW_STATIC + PH_RESV_KW_CLASS);
					string[] enumList = Regex.Match(newCode, "{(.*?)}", RegexOptions.Singleline).Value.Replace("{", "").Replace("}", "").Split(',');
					string results = "";
					for(int i = 0; i < enumList.Length; i++) {
						if(enumList[i].Trim() == "") continue;
						// If there was already a value assigned, get the value as new counter (if it was a numeric figure)
						if(enumList[i].IndexOf("=") != -1) {
							string preAssignedValue = Regex.Match(enumList[i], @"=.*?(\d+)$").Groups[1].Value;
							if(preAssignedValue != "") {
								enumCounter = Convert.ToInt32(preAssignedValue) + 1;
							}
							results += "public " + PH_RESV_KW_STATIC + PH_RESV_KW_INT + enumList[i].Trim() + ";" + Environment.NewLine;
						} else {
							results += "public " + PH_RESV_KW_STATIC + PH_RESV_KW_INT + enumList[i].Trim() + "=" + (enumCounter++).ToString() + ";" + Environment.NewLine;
						}
					}
					newCode = Regex.Replace(newCode, "{(.*?)}", "{" + results + "}");
					code = code.Replace(m.Value, newCode);
				}
				return code;
			}

			/// <summary>
			/// Add missing scope brackets (curly brackets) to "if" "else if" "while" "for"
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] AddMissingScopeBrackets(string[] codes) {
				List<string> codeList = codes.ToList<string>();
				for(int i = 0; i < codeList.Count; i++) {
					if(!RoundBracketScopeHolderKeywords.Contains<string>(codeList[i])) continue;
					int startRoundBracketIndex = -1;
					for(int j = i; j < codeList.Count; j++) {
						if(codeList[j] == "(") {
							startRoundBracketIndex = j;
							break;
						}
					}
					int endRoundBracketIndex = FindEndScopeBlock(codeList.ToArray<string>(), startRoundBracketIndex, "(", ")");
					if(codeList[endRoundBracketIndex + 1] == "{") {
						i = endRoundBracketIndex;
						continue;
					}

					int roundBracketCnt = 0;
					// No scope brackets found, so we will insert a pair
					// Search for the first end of statement (;) semi-colon and we will insert the ending scope bracket straight after this semi-colon
					for(int j = endRoundBracketIndex + 1; j < codeList.Count; j++) {
						// We need to ignore anything inside round brackets
						// For example, we can have statements like:
						// ----------------------------------------------
						// while(i == 0) for(int x = 0; x < 2; x++) i++;
						// ----------------------------------------------
						if(codeList[j] == "(") {
							roundBracketCnt++;
						} else if(codeList[j] == ")") {
							roundBracketCnt--;
						}
						if(codeList[j] != ";") continue;
						if(roundBracketCnt != 0) continue;

						codeList.Insert(j + 1, "}");
						codeList.Insert(endRoundBracketIndex + 1, "{");
						i = endRoundBracketIndex;
						break;
					}
				}
				codes = codeList.ToArray<string>();
				return codes;
			}

			/// <summary>
			/// Convert all occurance of "string" to "char[]"
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertStringKeywordToCharArray(string[] codes) {
				List<string> codeList = codes.ToList<string>();
				for(int i = 0; i < codeList.Count; i++) {
					// Convert all occurance of "string" to "char[]"
					if(codeList[i].Length >= PH_RESV_KW_STRING.Length) {
						if(codeList[i].Substring(0, PH_RESV_KW_STRING.Length) == PH_RESV_KW_STRING) {
							codeList[i] = ReplaceFirstOccurance(codeList[i], PH_RESV_KW_STRING, PH_RESV_KW_CHAR.Trim() + "[] ");
							codeList[i] = codeList[i].Trim();
							// If we have a string assginement operator straight after, we want to separate the declaration and assignment in to separate statements
							// Doing so will make the assignment codes consistent & easier for us to search into our codes later when separating strings inside double quotes to separated char arrays
							if(codeList[i + 1] == "=") {
								string[] sep = codeList[i].Split(' ');
								string strVariableName = sep[1];
								codeList[i + 1] = ";";
								codeList.Insert(i + 2, "=");
								codeList.Insert(i + 2, strVariableName);
							}
						}
					}
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Convert all char placeholders to actual decimal (UTF-16 / Decimal) representation
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertCharPlaceHolderToDecimal(string[] codes) {
				List<string> codeList = codes.ToList<string>();
				for(int i = 0; i < codeList.Count; i++) {
					if(codeList[i].IndexOf(PH_ID + PH_CHAR) == -1) continue;
					string str = CharLiteralList[codeList[i]];
					// String has something like: 'A', so remove single quotation from both sides
					str = str.Substring(1, str.Length - 2);
					// Convert '\{x}' to actual escape characters
					str = str.Replace("\\\\", "\\");
					str = str.Replace("\\n", "\n");
					str = str.Replace("\\t", "\t");
					str = str.Replace("\\0", "\0");
					ushort c = Convert.ToUInt16(str.ToCharArray()[0]);
					codeList[i] = c.ToString();
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Convert all string placeholders to char arrays assigned with actual decimal (UTF-16 / Decimal) representation
			/// IMPORTANT: Since we are allocating dynamic memory in the virtual heap, we must release the memory allocated after it has been used!
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertStringPlaceHolderToCharDecimalArray(string[] codes) {
				Dictionary<string, string> instanceObjList = GetInstanceObjects(codes);
				List<string> codeList = codes.ToList<string>();
				for(int i = 0; i < codeList.Count; i++) {
					if(codeList[i].IndexOf(PH_ID + PH_STR) == -1) continue;
					if(codeList[i - 1] != "=") continue;
					string varName = codeList[i - 2];
					//if(varName.IndexOf(TmpVarIdentifier) != -1) continue;
					string str = StringLiteralList[codeList[i]];
					// String has something like: "Hello" so, Remove double quotation from both sides
					str = str.Substring(1, str.Length - 2);
					// Convert "\{x}" to actual escape characters
					str = str.Replace("\\\\", "\\");
					str = str.Replace("\\n", "\n");
					str = str.Replace("\\t", "\t");
					str = str.Replace("\\0", "\0");
					str += "\0";

					bool isInstantiateObj = false;
					// Check if we already have the char array instantiated or not
					if(!instanceObjList.ContainsKey(varName)) {
						isInstantiateObj = true;
					} else {
						// If we have non-instantiated pointer variable
						if(instanceObjList[varName] + " " == PH_RESV_KW_PTR) {
							isInstantiateObj = true;
						}
					}
					// Instantiate object if we don't have it instantiated already
					if(isInstantiateObj) {
						codeList[i - 2] = varName;
						codeList[i - 1] = "=";
						codeList[i] = PH_ID + PH_NEW + PH_ID + PH_RESV_KW_CHAR.Trim() + "[" + str.Length + "]";
					}

					int arrayIndex = str.Length - 1;
					for(int j = str.Length - 1; j >= 0; j--) {
						ushort c = Convert.ToUInt16(str[j]);
						codeList.Insert(i + 2, ";");
						codeList.Insert(i + 2, c.ToString());
						codeList.Insert(i + 2, "=");
						codeList.Insert(i + 2, varName + "[" + (arrayIndex--).ToString() + "]");
					}

					/*
					// Now, since we are allocating dynamic memory in the virtual heap, we must release the memory allocated after it has been used!
					// Is this variable something that we auto-generated?
					if(varName.IndexOf(TmpVarIdentifier) != -1) {
						// If so, this variable will be used only once, so we find the place where this variable is used
						for(int j = i; j < codeList.Count; j++) {
							if(codeList[j] == varName) {
								// Search for the end of statement where this variable is used
								for(int k = j; k < codeList.Count; k++) {
									if(codeList[k] == ";") {
										// Found it! So, we will just insert a IASM code that releases memory for this variable
										codeList.Insert(k + 1, ";");
										codeList.Insert(k + 1, "PSH " + varName + " PSH " + (str.Length * 2).ToString() + " VMC DScript.VMC.VMC_FREE");
										break;
									}
								}
								break;
							}
						}
					}
					*/

					/*
					// Now, since we are allocating dynamic memory in the virtual heap, we must release the memory allocated after it has been used!
					// First of all, find the start of scope where this instantiation code belongs
					int bracketCnt = 0;
					int scopeStartIndex = 0;
					for(int j = i; j >= 0; j--) {
						if(codeList[j] == "}") {
							bracketCnt++;
							continue;
						}
						if(codeList[j] == "{" && bracketCnt != 0) {
							bracketCnt--;
							continue;
						}
						if(codeList[j] == "{" && bracketCnt == 0) {
							scopeStartIndex = j;
							break;
						}
					}
					int scopeEndIndex = FindEndScopeBlock(codeList.ToArray<string>(), scopeStartIndex, "{", "}");
					// Within the scope range, we find 
					// PSH DScript.VMC.Free.address PSH DScript.VMC.Free.numBytesToRelease VMC DScript.VMC.VMC_FREE
					codeList.Insert(scopeEndIndex - 1, "PSH " + varName + " PSH " + (str.Length * 2).ToString() + " VMC DScript.VMC.VMC_FREE");
					*/
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Replace pre-defined keywords (true, false, null, etc) with actual values (may be numeric, class objects, strings, etc)
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ReplacePreDefinedKeywordsWithActualValue(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == PH_RESV_KW_FALSE) {
						codes[i] = "0";
					} else if(codes[i] == PH_RESV_KW_TRUE) {
						codes[i] = "1";
					} else if(codes[i] == PH_RESV_KW_NULL) {
						codes[i] = "0";
					}
				}
				return codes;
			}

			/// <summary>
			/// Convert increment/decrement operators to placeholders
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string ConvertIncDecOperatorsToPlaceholders(string code) {
				List<string> operatorList = Operators.ToList<string>();
				string[] additionalSymbols = new string[] { ";", ",", ")", "(", "[", "]", "{", "}" };
				operatorList.InsertRange(0, additionalSymbols.ToList<string>());
				// Insert space after all operators
				for(int i = 0; i < operatorList.Count; i++) {
					// Ignore plus + and minus -
					if(operatorList[i] == "+" || operatorList[i] == "-" || operatorList[i] == "++" || operatorList[i] == "--") continue;
					code = code.Replace(operatorList[i], " " + operatorList[i] + " ");
				}
				// Convert ++ and -- to placeholders
				code = code.Replace(" ++", "＋");
				code = code.Replace("++ ", "＋");
				code = code.Replace(" --", "—");
				code = code.Replace("-- ", "—");

				// Revert back empty spaces
				for(int i = 0; i < operatorList.Count; i++) {
					// Ignore plus + and minus -
					if(operatorList[i] == "+" || operatorList[i] == "-" || operatorList[i] == "++" || operatorList[i] == "--") continue;
					code = code.Replace(" " + operatorList[i] + " ", operatorList[i]);
				}
				return code;
			}

			/// <summary>
			/// Since DScript language does not support float/double internally,
			/// if a floating number is found, we need to multiply it by 10^n to move the decimal point rightwards to make it a whole number
			///
			/// NOTE: Default decimal point precision is defined as below (alternatively, you can define the precision with the command line option -fp{x} where {x} is the amount to move the decimal point towards the right)
			/// 
			/// 16 bit signed variable (-32768 to 32767): float --> -3.2768 to 3.2767 (Therefore, multiply by 10000) 16 bit = 4 decimal places
			/// 32 bit signed variable (-2147483648 to 2147483647): float --> -2147.483648 to 2147.483647 (Therefore, multiply by 1000000) 32 bit = 6 decimal places
			/// 64 bit signed variable (-9223372036854775808 to 9223372036854775807): float --> -92233720368.54775808 to 92233720368.54775807 (Therefore, multiply by 100000000) 64 bit = 8 decimal places
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="suffix"></param>
			/// <param name="multiplyBy"></param>
			/// <returns></returns>
			private string[] ResolveFloatDoubleSuffix(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					// If a string does not contain a suffix at the last character, ignore
					char[] suffix = new char[] { 'f', 'F', 'd', 'D' };
					bool isSuffixExists = false;
					for(int j = 0; j < suffix.Length; j++) {
						if(codes[i][codes[i].Length - 1] == suffix[j]) isSuffixExists = true;
					}
					if(!isSuffixExists) continue;
					// If not a numeric value, ignore
					string numberCheck = codes[i].Substring(0, codes[i].Length - 1).Replace(".", "");
					if(!Regex.IsMatch(numberCheck, @"^[0-9]+$")) continue;
					// Convert to a double, then multiply with 10^n depedning on the architecture
					double value = Convert.ToDouble(codes[i].Substring(0, codes[i].Length - 1));
					long normalizedValue = 0;
					normalizedValue = (long)(value * DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY);
					if(TARGET_ARCH_BIT_SIZE == 16) {
						codes[i] = Convert.ToInt16(normalizedValue).ToString();
					} else if(TARGET_ARCH_BIT_SIZE == 32) {
						codes[i] = Convert.ToInt32(normalizedValue).ToString();
					} else if(TARGET_ARCH_BIT_SIZE == 64) {
						codes[i] = Convert.ToInt64(normalizedValue).ToString();
					}
				}
				return codes;
			}

			/// <summary>
			/// Recursively resolve the actual class members that the variable is refering
			/// N.Program.Main.a.X  ----> N.INT32.X
			/// N.Program.Main.a.X.X  ----> N.INT32.X.X  ----> N.INT32.X
			/// N.Program.Main.a.X.Y  ----> N.INT32.X.Y  ----> N.INT32.Y
			/// </summary>
			/// <param name="varToResolve">Prerequisite: All variables' scope needs to be resolved</param>
			/// <param name="instanceObjList">Generate using GetInstanceObjects(codes);</param>
			/// <returns></returns>
			private string GetResolvedNestedInstanceVar(string varToResolve, Dictionary<string, string> instanceObjList) {
				string orgVarToResolve = varToResolve;
				// Store the last value separated by period, as this is the actual member/method the varToResolve is referencing
				string[] sep = varToResolve.Split('.');
				string member = sep[sep.Length - 1];
				varToResolve = "";
				for(int i = 0; i < sep.Length; i++) varToResolve += sep[i] + ".";
				varToResolve = varToResolve.Substring(0, varToResolve.Length - 1);
			repeatVarResolveSearch:
				foreach(var value in instanceObjList) {
					if(varToResolve.IndexOf(value.Key) != -1) {
						varToResolve = varToResolve.Replace(value.Key, value.Value);
						goto repeatVarResolveSearch;
					}
				}
				if(orgVarToResolve != varToResolve) varToResolve += "." + member;
				return varToResolve;
			}

			/// <summary>
			/// Resolve floating function calls that usually returns some kind of value back to the caller
			/// NOTE: Our functional always returns some kind of value, even with the void function - will return 0
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveFloatingFunctionCalls(string[] codes) {
				// Get all function list and store in a dictionary with: (function name, return type)
				Dictionary<string, string> funcList = GetAllFunctionList(codes);
				// Get all the instance object list (instance obj name, class name)
				Dictionary<string, string> instanceObjList = GetInstanceObjects(codes);

				// Search for codes that do not contain preserved keywords, and check if it is a function call or not
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length != 1) continue;
					if(AllReservedKeywords.Contains<string>(codes[i] + " ")) continue;
					string actualFunction = Regex.Replace(codes[i], @"\[(.*?)\]", "");
					// Check if function exists
					if(!funcList.ContainsKey(actualFunction)) {
						// If function does not exist, do a secondary check if this function call is from an instance object
						// First, check if the function caller is an instance object or not (N.C.a.add(xxx); --> where "N.C.a" is an instance object)
						string[] funcNameSplit = actualFunction.Split('.');
						string checkInstObj = "";
						for(int j = 0; j < funcNameSplit.Length - 1; j++) checkInstObj += funcNameSplit[j] + ".";
						if(checkInstObj == "") continue;
						checkInstObj = checkInstObj.Substring(0, checkInstObj.Length - 1);
						if(!instanceObjList.ContainsKey(checkInstObj)) {
							// Try resolving the instance object and generate the actual class member hierarchy path
							checkInstObj = GetResolvedNestedInstanceVar(checkInstObj, instanceObjList);
							if(!instanceObjList.ContainsKey(checkInstObj)) continue;
						}
						// If the caller was an instance object, check if function exist
						actualFunction = instanceObjList[checkInstObj] + "." + funcNameSplit[funcNameSplit.Length - 1];
						if(!funcList.ContainsKey(actualFunction)) continue;
					}
					// If it is a function, then check for any assignment operators
					if(codes[i - 1] == "=") continue;
					// If assignment operator does not exist, add an assignment code
					string assignmentCode = "";
					// Create a new variable to store the return variable
					string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
					// If function return type is a void, the return type is byte with return code 0
					if(funcList[actualFunction] + " " == PH_RESV_KW_VOID) {
						//assignmentCode = PH_VAR + "s" + TARGET_ARCH_BIT_SIZE + " " + REG_SVAR0 + "=";
						assignmentCode = PH_RESV_KW_BYTE + tmpVar + ";" + tmpVar + "=";
					} else {
						assignmentCode = funcList[actualFunction] + " " + tmpVar + ";" + tmpVar + "=";
					}
					// Before we add the assignmentCode, we need to check if this function call is inside an expression
					// (e.g. byte results = i < add(1);   <----   where add(1) is the function call that we are going to operate on)
					// We can tell if this is an expression or not by traversing back until we find an end of statement semicolon (;),
					// And if we encounter an assignment operator (=) before reaching the semicolon, it means this function call is within an expression
					// Thus, we will need to move this function call before the beginning of the expression
					bool isFuncCallInsideExpression = false;
					int endOfLastStatementIndex = 0;
					for(int j = i - 1; j >= 0; j--) {
						if(codes[j] == "=") {
							isFuncCallInsideExpression = true;
							continue;
						}
						if(codes[j] == ";") {
							endOfLastStatementIndex = j;
							break;
						}
					}

					// If the function call is inside an expression, we don't need to add a temporary placeholder for the return value
					// Because function calls inside an expression must ALWAYS return something!
					if(isFuncCallInsideExpression) continue;
					// Otherwise, create a temporary placeholder variable to store the returning value of this function call (so we can safely POP the value PUSHed to the stack)
					codes[i] = assignmentCode + codes[i];

					/*
					// This function call is inside an expression
					if(isFuncCallInsideExpression) {
						// Get the end of function call
						int endRoundBracketIndex = FindEndScopeBlock(codes, i + 1, "(", ")");
						int funcCallStatementEndIndex = 0;
						// Get the semicolon index after the functionall (so we are getting the full statement containing the function call here)
						for(int j = endRoundBracketIndex; j < codes.Length; j++) {
							if(codes[j] == ";") {
								funcCallStatementEndIndex = j;
								break;
							}
						}
						// Generate assignment code
						for(int j = i; j <= funcCallStatementEndIndex; j++) {
							// Insert resolved function call statement just after the previous statement
							assignmentCode += codes[j];
							// As we assign the function call statement, remove the code
							codes[j] = "";
						}
						codes[endOfLastStatementIndex] += assignmentCode;
						// Replace the function call statement with the temporary variable that contains the results of the function call
						codes[funcCallStatementEndIndex - 1] = tmpVar;
						codes[funcCallStatementEndIndex] = ";";
						// 
					} else {
						codes[i] = assignmentCode + codes[i];
					}*/

				}
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// Gets all the functions in the code and return it in (function name, return type) Dictionary list
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private Dictionary<string, string> GetAllFunctionList(string[] codes) {
				Dictionary<string, string> funcList = new Dictionary<string, string>();
				// Get all function list and store in a dictionary with: (function name, return type)
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(!AllFunctionKeywords.Contains<string>(splitCode[0] + " ")) continue;
					string[] funcSplit = splitCode[0].Split('_');
					// If this is a function that returns a pointer, replace it with "ptr𒀭" instead
					if(splitCode[0] == PH_RESV_KW_FUNCTION_PTR.Trim()) {
						funcSplit[1] = PH_RESV_KW_PTR.Trim();
					}
					funcList.Add(splitCode[1], funcSplit[1]);
				}
				return funcList;
			}

			/// <summary>
			/// For each "return(xxx);" statement, separate equation so we only have one variable inside the return bracket: e.g. "var ret = xxx; return(ret);"
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveReturnStatements(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					string[] funcSplit = codes[i].Split(' ');
					if(!AllFunctionKeywords.Contains<string>(funcSplit[0] + " ")) continue;
					// Get the function return type (function has "function_typename" format)
					string[] funcNameSplit = funcSplit[0].Split('_');
					string funcType = funcNameSplit[1];
					int endRoundBracketScopeIndex = FindEndScopeBlock(codes, i, "(", ")");
					int startScopeBlockIndex = endRoundBracketScopeIndex + 1;
					int endScopeBlockIndex = FindEndScopeBlock(codes, startScopeBlockIndex, "{", "}");
					// For all "return" statements found, get the contents inside the round brackets
					if(funcType + " " == PH_RESV_KW_VOID) funcType = PH_RESV_KW_BYTE;
					for(int j = startScopeBlockIndex; j < endScopeBlockIndex; j++) {
						if(codes[j] + " " == PH_RESV_KW_RETURN) {
							// If "return (0);", we keep it as-is
							if(codes[j + 2] == "0" && codes[j + 3] == ")") continue;
							int endReturnBlockIndex = FindEndScopeBlock(codes, j + 1, "(", ")");
							// Create a new variable to store the return variable
							string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
							string expression = funcType + " " + tmpVar + "=";
							if(funcSplit[0] + " " == PH_RESV_KW_FUNCTION_PTR) {
								expression = PH_RESV_KW_PTR + tmpVar + "=";
							}
							for(int k = j + 2; k < endReturnBlockIndex; k++) {
								expression += codes[k];
								codes[k] = "";
							}
							codes[j + 2] = tmpVar;
							codes[j] = expression + ";" + PH_RESV_KW_RETURN;
							j = endReturnBlockIndex;
						}
					}
				}
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// Add "return(0);" to the very bottom of functions (ALL functions must return somthing, even if it is a void function)
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] AddReturnToFunctions(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					string[] funcSplit = codes[i].Split(' ');
					if(!AllFunctionKeywords.Contains<string>(funcSplit[0] + " ")) continue;
					int endRoundBracketScopeIndex = FindEndScopeBlock(codes, i, "(", ")");
					int startScopeBlockIndex = endRoundBracketScopeIndex + 1;
					int endScopeBlockIndex = FindEndScopeBlock(codes, startScopeBlockIndex, "{", "}");
					// We only need to add return 0; to void functions
					if(funcSplit[0] + " " == PH_RESV_KW_FUNCTION_VOID) {
						// Add final "return 0;" statement to void functions
						codes[endScopeBlockIndex] = PH_RESV_KW_RETURN + "(0);" + "}";
						// Now, for all "return;" statements inside the void function, we convert them to "return(0);"
						for(int j = startScopeBlockIndex; j < endScopeBlockIndex; j++) {
							if(codes[j] + " " == PH_RESV_KW_RETURN) {
								codes[j + 1] = "(0);";
							}
						}
					}
				}
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// Given a variable name, this method returns the list of right side of the assignment operator
			/// e.g.
			/// var a = 10;
			/// var a = b + 20;
			/// --->
			/// [0] 10
			/// [1] b + 20
			/// </summary>
			/// <param name="variableName"></param>
			/// <returns></returns>
			private List<string> GetListOfVariableAssignment(string[] codes, string variableName) {
				List<string> retList = new List<string>();
				// For all codes, search for the assignemnt symbol
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != "=") continue;
					string leftOfOperator = codes[i - 1];
					string[] splitCode = codes[i - 1].Split(' ');
					if(splitCode.Length > 1) leftOfOperator = splitCode[1];
					if(leftOfOperator != variableName) continue;
					string expression = GetFullExpressionStatementFromAssignmentOperator(codes, i) + ";";
					string rightOfOperator = Regex.Match(expression, @"(?<==).+?(?=;)").Value;
					retList.Add(rightOfOperator);
				}
				return retList;
			}

			/// <summary>
			/// Get all variables inside the code that is assigned with values, and store each variable & the list of its possible expression(s)
			/// Get these:
			/// var i = 0;
			/// int x = 1;
			/// But NOT declaration-only variables:
			/// var z;
			/// int y;
			/// </summary>
			/// <param name="codes"></param>
			/// <returns>List of variables (varname, [list of expressions])  that are assigned with values</returns>
			private Dictionary<string, List<string>> GetAllValueAssignedVariablesInCode(string[] codes) {
				Dictionary<string, List<string>> allValueAssignedVariableList = new Dictionary<string, List<string>>();
				for(int i = 0; i < codes.Length; i++) {
					// For all codes, search for the assignemnt symbol
					if(codes[i] != "=") continue;
					// Check if not a logical operator
					if(!Regex.IsMatch(codes[i - 1], @"[<|>|!|=]") && codes[i + 1] != "=") {
						string leftOfOperator = codes[i - 1];
						string[] splitCode = codes[i - 1].Split(' ');
						if(splitCode.Length > 1) {
							leftOfOperator = splitCode[1];
						} else {
							leftOfOperator = splitCode[0];
						}
						if(!allValueAssignedVariableList.ContainsKey(leftOfOperator)) {
							allValueAssignedVariableList.Add(leftOfOperator, new List<string>());
						}
						string expression = GetFullExpressionStatementFromAssignmentOperator(codes, i) + ";";
						string rightOfOperator = Regex.Match(expression, @"(?<==).+?(?=;)").Value;
						allValueAssignedVariableList[leftOfOperator].Add(rightOfOperator);
					}
				}
				return allValueAssignedVariableList;
			}

			/// <summary>
			/// For all "var" keywords, determine their appropriate size if possible - if not able to determine, it will be converted to the most largest variable type (long)
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveImplicitVarType(string[] codes) {
				// Get all variables inside the code that are assigned with values
				Dictionary<string, List<string>> allValueAssignedVariableList = GetAllValueAssignedVariablesInCode(codes);

				// Search inside the entire code list for var declaration
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length == 1) continue;
					if(splitCode[0] + " " == PH_RESV_KW_VAR) {
						// Now search into the entire code and get all of this variable with assignment operator
						string varToCheck = splitCode[1];
						//List<string> listOfVarAssignment = GetListOfVariableAssignment(codes, varToCheck);

						List<string> listOfVarAssignment = allValueAssignedVariableList[varToCheck];

						// If there's no variable assignments, it means this is a variable that is declared but nothing is assigned
						// We will set this variable as the maximum value type possbile (long)
						if(listOfVarAssignment == null || listOfVarAssignment.Count == 0) {
							codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_LONG);
							continue;
						}

						// If the variable name contains "RQ20131212DSRNDID", then it means it's an auto generated temporary variable
						// In any case, we just declare it as the maximum value type possible (double)
						if(varToCheck.IndexOf(TmpVarIdentifier) != -1) {
							// If we have an end of statement in the next code, no need to add varToCheck
							if(codes[i + 1] == ";") {
								varToCheck = "";
							} else {
								varToCheck = ";" + varToCheck;
							}
							codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_LONG) + varToCheck;
							continue;
						}

						// If we have at least one string placeholder, the variable is a string
						for(int j = 0; j < listOfVarAssignment.Count; j++) {
							if(listOfVarAssignment[j].IndexOf(PH_ID + PH_STR) != -1) {
								ActualClassMemberPointerVarTypeList.Add(varToCheck, PH_RESV_KW_CHAR.Trim());
								// If we have an end of statement in the next code, no need to add varToCheck
								if(codes[i + 1] == ";") {
									varToCheck = "";
								} else {
									varToCheck = ";" + varToCheck;
								}
								codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_PTR) + varToCheck;
								continue;
							}
						}

						// If there's an assignment of "new" keyword, then this variable type is a pointer, and will be the most maximum value allowed (so an unsigned long)
						bool isNewKeyword = false;
						for(int j = 0; j < listOfVarAssignment.Count; j++) {
							if(listOfVarAssignment[j].IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
								isNewKeyword = true;
								break;
							}
						}
						if(isNewKeyword) {
							codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_ULONG);
							// If the expression contains an assignment operator (=), then separate declaration and assignment expressions
							if(codes[i + 1].IndexOf("=") != -1) {
								codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_ULONG, PH_RESV_KW_ULONG + varToCheck + ";");
							}
							continue;
						}


						// Now check if the list contains any non-numeric values (such as variables, strings, function calls, arrays, etc)
						bool isDecimalValue = false;
						for(int j = 0; j < listOfVarAssignment.Count; j++) {
							if(listOfVarAssignment[j].IndexOf(".") != -1) {
								isDecimalValue = true;
								if(Regex.IsMatch(listOfVarAssignment[j], @"\(|\[")) {
									isDecimalValue = false;
									break;
								}
							}
						}
						if(isDecimalValue) {
							// Note float / double will be converted to double
							codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_DOUBLE);
							// If the expression contains an assignment operator (=), then separate declaration and assignment expressions
							if(codes[i + 1].IndexOf("=") != -1) {
								codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_DOUBLE, PH_RESV_KW_DOUBLE + varToCheck + ";");
							}
							continue;
						}

						// Now check if the list contains any non-numeric values (such as variables, strings, function calls, arrays, etc)
						bool isAllNumeric = true;
						for(int j = 0; j < listOfVarAssignment.Count; j++) {
							try {
								Convert.ToInt64(listOfVarAssignment[j]);
							} catch {
								isAllNumeric = false;
							}
						}



						// We cannot determine the maximum/minimum size of this variable, so just make it the largest possible variable size
						if(!isAllNumeric) {
							codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, PH_RESV_KW_LONG);
							// If the expression contains an assignment operator (=), then separate declaration and assignment expressions
							if(codes[i + 1].IndexOf("=") != -1) {
								codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_LONG, PH_RESV_KW_LONG + varToCheck + ";");
							}
							continue;
						}
						// Sort by ASC
						listOfVarAssignment.Sort();
						long min = Convert.ToInt64(listOfVarAssignment[0]);
						long max = Convert.ToInt64(listOfVarAssignment[listOfVarAssignment.Count - 1]);
						string determinedVarType = PH_RESV_KW_LONG;

						if(TARGET_ARCH_BIT_SIZE == 16) {
							if(min >= 0 && max <= 255) {
								determinedVarType = PH_RESV_KW_BYTE;
							} else if(min >= -128 && max <= 127) {
								determinedVarType = PH_RESV_KW_SBYTE;
							} else if(min >= 0 && max <= 65535) {
								determinedVarType = PH_RESV_KW_USHORT;
							} else if(min >= -32768 && max <= 32767) {
								determinedVarType = PH_RESV_KW_SHORT;
							}
						} else if(TARGET_ARCH_BIT_SIZE == 32) {
							if(min >= 0 && max <= 255) {
								determinedVarType = PH_RESV_KW_BYTE;
							} else if(min >= -128 && max <= 127) {
								determinedVarType = PH_RESV_KW_SBYTE;
							} else if(min >= 0 && max <= 65535) {
								determinedVarType = PH_RESV_KW_USHORT;
							} else if(min >= -32768 && max <= 32767) {
								determinedVarType = PH_RESV_KW_SHORT;
							} else if(min >= 0 && max <= 4294967295) {
								determinedVarType = PH_RESV_KW_UINT;
							} else if(min >= -2147483648 && max <= 2147483647) {
								determinedVarType = PH_RESV_KW_INT;
							}
						} else if(TARGET_ARCH_BIT_SIZE == 64) {
							if(min >= 0 && max <= 255) {
								determinedVarType = PH_RESV_KW_BYTE;
							} else if(min >= -128 && max <= 127) {
								determinedVarType = PH_RESV_KW_SBYTE;
							} else if(min >= 0 && max <= 65535) {
								determinedVarType = PH_RESV_KW_USHORT;
							} else if(min >= -32768 && max <= 32767) {
								determinedVarType = PH_RESV_KW_SHORT;
							} else if(min >= 0 && max <= 4294967295) {
								determinedVarType = PH_RESV_KW_UINT;
							} else if(min >= -2147483648 && max <= 2147483647) {
								determinedVarType = PH_RESV_KW_INT;
							} else if(min >= -9223372036854775808 && max <= 9223372036854775807) {
								determinedVarType = PH_RESV_KW_LONG;
							} else if(min >= 0 && max > 9223372036854775807) {
								determinedVarType = PH_RESV_KW_ULONG;
							}
						}

						codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_VAR, determinedVarType);
						// If the expression contains an assignment operator (=), then separate declaration and assignment expressions
						if(codes[i + 1].IndexOf("=") != -1) {
							codes[i] = ReplaceFirstOccurance(codes[i], determinedVarType, determinedVarType + varToCheck + ";");
						}
						continue;
					}
				}

				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// For each class, move all member variable initiation codes to all declared constructor(s), and variable assignments to top of class
			/// Prerequisite: At least one constructor must be created beforehand
			/// 
			/// e.g.
			/// class C {
			///		int a;
			///		a = 0;
			///		int xxxx;
			///		function C() {}
			///		function C(int a) {}
			///		int b;
			///		b = 0;
			/// }
			///
			/// -------> MERGE/CONVERT TO ------>
			///
			/// class C {
			///		int a;
			///		int b;
			///		function C() {
			///			// If "int xxxx" is a temporary var which is only used in the constructor, we can just declare it inside the constructor
			///			// We need to do this, because all class member variables become pointers at a later stage and we don't want dummy variables to be so
			///			int xxxx = 1;
			///			a = 0;
			///			b = 0;
			///		}
			///		function C(int a) {
			///			a = 0;
			///			b = 0;
			///		}
			/// }
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] RestructureClassMemberVars(string[] codes, string keyword) {
				// Normalize namespace's classes
				foreach(int index in GenerateKeywordIndexList(codes, keyword)) {
					string classWithNameSpaceName = ReplaceFirstOccurance(codes[index], keyword, "");
					string[] classNameSplit = classWithNameSpaceName.Split('.');
					string className = classNameSplit[classNameSplit.Length - 1];
					//int constructorMemberInsertionIndex = -1;
					int classMemberDeclarationInsertionindex = -1;
					string initiationCode = "";
					string declarationCode = "";
					// Next, we need to find where the parent scope block ends
					int endClassScopeBlock = FindEndScopeBlock(codes, index, "{", "}");
					// And we need to find where the class scope actually begins - this is where we begin inserting class member variable declaratinos
					for(int i = index + 1; i < endClassScopeBlock; i++) {
						if(codes[i] == "{") {
							classMemberDeclarationInsertionindex = i;
							break;
						}
					}
					List<int> constructorMemberInsertionIndex = new List<int>();
					for(int i = classMemberDeclarationInsertionindex + 1; i < endClassScopeBlock; i++) {
						// Ignore any colons and scope brackets
						//if(Regex.IsMatch(codes[i], "{|}|:")) continue;
						// If function found, we ignore
						// However, if constructor found, we will keep its index where we will insert the member initiation code

						string[] splitFuncName = codes[i].Split(' ');
						if(AllFunctionKeywords.Contains<string>(splitFuncName[0] + " ")) {

							//if(codes[i].IndexOf(PH_RESV_KW_FUNCTION, 0) != -1) {
							int endFunctionParamBlock = FindEndScopeBlock(codes, i, "(", ")");
							if(codes[i] == splitFuncName[0] + " " + classWithNameSpaceName + "." + className) {
								// Constructor found, so store the index where we will insert the member initiation code
								constructorMemberInsertionIndex.Add(endFunctionParamBlock + 1);
							}
							// Skip search for all function scope block
							int endFunctionScopeBlock = FindEndScopeBlock(codes, endFunctionParamBlock, "{", "}");
							i = endFunctionScopeBlock;
							continue;
						}

						// Ignore if we have a label (NOP labelname;) --> (e.g. We might have labels on top of constructors: [DScript_Init] --> 𒀭PH_LBL𒀭DScriptMain;)
						if(splitFuncName[0].IndexOf(PH_ID + PH_LABEL + PH_ID) != -1) continue;

						// An assignemnt operator was NOT found in the next code, so this statement is a variable declaration code
						if(codes[i + 1] != "=") {
							// Anything other than If variable declaration code found, we store it and skip until end of statement
							if(codes[i].IndexOf(" ") != -1) {
								for(int j = i; j < endClassScopeBlock; j++) {
									declarationCode += codes[j];
									if(codes[j] == ";") {
										codes[j] = "";
										i = j;
										break;
									}
									codes[j] = "";
								}
							}
							continue;
						}
						// An assignemnt operator was found in the next code, so this statement is a variable assignment code
						// For anything other than variable declaration, we regard & store it as initiation code
						for(int j = i; j < endClassScopeBlock; j++) {
							initiationCode += codes[j];
							if(codes[j] == ";") {
								codes[j] = "";
								i = j;
								break;
							}
							codes[j] = "";
						}
					}
					// Insert variable assignments to constructor
					for(int i = 0; i < constructorMemberInsertionIndex.Count; i++) {
						codes[constructorMemberInsertionIndex[i]] += " " + initiationCode;
					}
					// Insert variable declarations to top of class
					codes[classMemberDeclarationInsertionindex] += " " + declarationCode;
				}
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);

				return codes;
			}

			private string[] MoveTemporaryClassMemberVarToConstructor(string[] codes, string keyword) {
				// For each class...
				foreach(int index in GenerateKeywordIndexList(codes, keyword)) {
					string classWithNameSpaceName = ReplaceFirstOccurance(codes[index], keyword, "");
					string[] classNameSplit = classWithNameSpaceName.Split('.');
					string className = classNameSplit[classNameSplit.Length - 1];
					int classMemberDeclarationInsertionIndex = -1;
					string constructor = PH_RESV_KW_FUNCTION_VOID + classWithNameSpaceName + "." + className;
					// Next, we need to find where the parent scope block ends
					int endClassScopeBlock = FindEndScopeBlock(codes, index, "{", "}");
					int constructorIndex = -1;
					// And we need to find where the class scope actually begins - this is where class member variable declarations are found
					// We will also search the code index where the constructor is found
					for(int i = index + 1; i < endClassScopeBlock; i++) {
						if(codes[i] == "{") {
							classMemberDeclarationInsertionIndex = i;
							continue;
						}
						if(codes[i] == constructor) {
							constructorIndex = i;
							break;
						}
					}
					// If we don't have any constructors, quit
					if(constructorIndex == -1) continue;

					// Find the insertion position in the constructor
					int constructorMemberVarInsertionIndex = -1;
					for(int i = constructorIndex; i < endClassScopeBlock; i++) {
						if(codes[i] == "{") {
							constructorMemberVarInsertionIndex = i;
							break;
						}
					}

					// Get the list of declared member variables
					// Note that all class member variables are already moved to the top of class, so we can safely stop searching until the first function is found
					Dictionary<string, int> declaredMemberVarList = new Dictionary<string, int>(); // (VarName, index in code)
					int firstFunctionFoundIndex = endClassScopeBlock; // There might be no functions in our class (there's a possibility we don't have constructors too)
					for(int i = classMemberDeclarationInsertionIndex + 1; i < endClassScopeBlock; i++) {
						string[] splitCode = codes[i].Split(' ');
						if(splitCode.Length < 2) continue;
						// First function found, which means we won't see any member var declarations beyond this line, so quit search
						if(AllFunctionKeywords.Contains<string>(splitCode[0] + " ")) {
							firstFunctionFoundIndex = i;
							break;
						}
						// Ignore if we have a label (NOP labelname;) --> (e.g. We might have labels on top of functions: e.g. [DScript_Init] --> 𒀭PH_LBL𒀭DScriptMain;)
						if(splitCode[0].IndexOf(PH_ID + PH_LABEL + PH_ID) != -1) continue;
						// Member variable is added to our dictionary list
						declaredMemberVarList.Add(splitCode[1], i);
					}

					//TmpVarIdentifier

					/*
					// Search inside the entire class for all class member variables
					for(int i = firstFunctionFoundIndex; i < endClassScopeBlock; i++) {
						// Ignore anything inside constructors
						if(codes[i] == constructor) {
							int endOfConstructor = FindEndScopeBlock(codes, i, "{", "}");
							i = endOfConstructor;
							continue;
						}
						// There might be multiple codes (including IASM codes) separated by a space so we need to break down code first
						string[] splitCode = codes[i].Split(' ');
						for(int j = 0; j < splitCode.Length; j++) {
							if(declaredMemberVarList.ContainsKey(splitCode[j])) {
								// If found at least one occurance of use of this variable, just remove it from the declared member var list
								declaredMemberVarList.Remove(splitCode[j]);
								break;
							}
						}
					}*/

					// Remove any member variable declaration from top of our class that should not be part of instantiation (i.e. temporary variables should not be included as global class member vars)
					// For such variables, we simply move each of the declarations in the list to the constructor code
					string declarationCode = "";
					foreach(var value in declaredMemberVarList) {
						if(value.Key.IndexOf(TmpVarIdentifier) == -1) continue;
						// Add to our declaration code
						declarationCode += codes[value.Value] + ";";
						// Remove declaration
						codes[value.Value] = "";
					}

					// Insert the declaration to the beginning of constructor code
					codes[constructorMemberVarInsertionIndex] += " " + declarationCode;
				}

				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);

				return codes;
			}

			/// <summary>
			/// Converts for loops to while loops
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] PreProcessForBlock(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == PH_RESV_KW_FOR) {
						codes = ConvertForToWhileLoop(codes, i);
						// Since position of "while" statement has been shifted as a result of ConvertForToWhileLoop, we will goto the new shifted code index
						for(int j = i; j < codes.Length; j++) {
							if(codes[j] == PH_RESV_KW_FOR) {
								// Convert this for statement to while statement
								codes[j] = PH_RESV_KW_WHILE;
								// Then, start searching for another for statement block after this
								i = j;
								break;
							}
						}
					}
				}
				return codes;
			}

			/// <summary>
			/// Processes the following for a single while loop block:
			/// Place the equation for logical comparison inside while statement outside the while brackets,
			/// and also place a label for the start and end block of the while statement
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startCodeIndex"></param>
			/// <returns></returns>
			private string[] ConvertForToWhileLoop(string[] codes, int startCodeIndex) {
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);

				// First of all, let's find out the parent scope of this while statement block
				// e.g.
				// {  <-----------------  we need to find where the scope for this while statement begins
				//		for(...) {}
				// }  <-----------------  we also need to find where the scope for this while statement ends
				// To do this, we will need to trace back the line of scopes until we reach the start of scope bracket "{"
				int startParentScopeBlock = 0;
				for(int i = startCodeIndex; i >= 0; i--) {
					if(codeList[i] == "{") {
						startParentScopeBlock = i;
						break;
					}
				}
				// Next, we need to find where the parent scope block ends
				int endParentScopeBlock = FindEndScopeBlock(codes, startParentScopeBlock, "{", "}");


				// Let's get the equation for logical comparison inside the round brackets
				int forConditionalStatementEnd = FindEndScopeBlock(codes, startCodeIndex + 1, "(", ")");
				int scopeBlockEndIndex = FindEndScopeBlock(codes, forConditionalStatementEnd + 1, "{", "}");


				// Get the initializer
				string initializer = "";
				int equationIndex = 0;
				for(int i = startCodeIndex + 2; i < forConditionalStatementEnd; i++) {
					initializer += codes[i];
					codeList[i] = "";
					if(codes[i] == ";") {
						// Go to the end of conditional statement (the conditional statement reamins inside the round brackets as-is, but with semi colon removed)
						for(int j = i + 1; j < forConditionalStatementEnd; j++) {
							if(codes[j] == ";") {
								codeList[j] = "";
								equationIndex = j + 1;
							}
						}
						codeList[i] = "";
						break;
					}
				}
				// Ignore the condition
				string loopEquation = "";
				for(int i = equationIndex; i < forConditionalStatementEnd; i++) {
					loopEquation += codes[i];
					codeList[i] = "";
				}
				loopEquation += ";";

				// Add initializer statement to the top of for statement
				codeList.Insert(startCodeIndex, initializer);
				// Add loop equation at the end of for scope
				codeList.Insert(scopeBlockEndIndex + 1, loopEquation);

				// Now, inside this for loop, we convert "break" and "continue" to "goto" statements
				for(int i = forConditionalStatementEnd + 3; i < scopeBlockEndIndex + 2; i++) {
					// if we encounter another while statement inside our while scope block, ignore any code inside it
					if(codeList[i] == PH_RESV_KW_WHILE || codeList[i] == PH_RESV_KW_FOR) {
						int endNestedWhileBlock = FindEndScopeBlock(codeList.ToArray<string>(), i, "{", "}");
						i = endNestedWhileBlock;
						continue;
					}
					// For loop continue statement behaves a bit differently than while loops, where for loop continue actually executes the loop equation
					if(codeList[i] == PH_RESV_KW_CONTINUE) {
						codeList[i] = loopEquation + PH_RESV_KW_CONTINUE;
					}
				}

				// Let's clean the codes again by separating into tokens
				string newCodes = "";
				for(int i = 0; i < codeList.Count; i++) newCodes += codeList[i];
				string[] newCodesArray = splitCodesToArray(newCodes);
				return newCodesArray;
			}


			/// <summary>
			/// Preprocess while loop statements by convertinf them to "if" statements,
			/// and also place a label for the start and end block of the while statement, along with converting "break" and "continue" to goto statements
			/// e.g.
			/// while(a == b) { continue; break; }
			/// -->
			/// startlabel: byte f; f = a == b; if(f) { goto startlabel; goto endlabel; goto startlabel; } endlabel:
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] PreProcessWhileBlock(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == PH_RESV_KW_WHILE) {
						codes = SeparateWhileExpressionsAndAddLabel(codes, i);
						// Since position of "while" statement has been shifted as a result of SeparateWhileExpressionsAndAddLabel, we will goto the new shifted code index
						for(int j = i; j < codes.Length; j++) {
							if(codes[j] == PH_RESV_KW_WHILE) {
								// Convert this while statement to if statement
								codes[j] = PH_RESV_KW_IF;
								// Then, start searching for another while statement block after this
								i = j;
								break;
							}
						}
					}
				}
				return codes;
			}

			/// <summary>
			/// Processes the following for a single while loop block:
			/// Preprocess while loop statements by convertinf them to "if" statements, and adding labels for start and end block of the entire while statement
			/// and also place a label for the start and end block of the while statement
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startCodeIndex"></param>
			/// <returns></returns>
			private string[] SeparateWhileExpressionsAndAddLabel(string[] codes, int startCodeIndex) {
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);

				// First of all, let's find out the parent scope of this while statement block
				// e.g.
				// {  <-----------------  we need to find where the scope for this while statement begins
				//		while(...) {}
				// }  <-----------------  we also need to find where the scope for this while statement ends
				// To do this, we will need to trace back the line of scopes until we reach the start of scope bracket "{"
				int startParentScopeBlock = 0;
				for(int i = startCodeIndex; i >= 0; i--) {
					if(codeList[i] == "{") {
						startParentScopeBlock = i;
						break;
					}
				}
				// Next, we need to find where the parent scope block ends
				int endParentScopeBlock = FindEndScopeBlock(codes, startParentScopeBlock, "{", "}");

				// An unique ID to be used for the while block
				string whileID = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

				// Let's get the equation for logical comparison inside the round brackets
				int logicalComparisonEQ = FindEndScopeBlock(codes, startCodeIndex + 1, "(", ")");
				int scopeBlockEndIndex = FindEndScopeBlock(codes, logicalComparisonEQ + 1, "{", "}");
				// Create a new variable to store the logical comparison expression
				//string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true);

				// Generate logical comparison expression outside the "while" keyword
				// Add beginning of while scope block label before the logical expression
				codeList[startCodeIndex] = PH_ID + PH_LABEL + PH_ID + "while_block_begin_" + whileID + ";" + PH_RESV_KW_WHILE;

				// add a goto statement to goback to the top of the while block and an end label for this while statement block
				codeList[scopeBlockEndIndex] = PH_RESV_KW_GOTO + PH_ID + PH_LABEL + PH_ID + "while_block_begin_" + whileID + ";" + "} " + PH_ID + PH_LABEL + PH_ID + "while_block_end_" + whileID + ";";

				// Now, inside this while loop, we convert "break" and "continue" to "goto" statements
				for(int i = logicalComparisonEQ + 2; i < scopeBlockEndIndex; i++) {
					// if we encounter another while statement inside our while scope block, ignore any code inside it
					// Note: that we have already converted for loops to while loops, so there won't be any for loops that we need to consider
					if(codeList[i] == PH_RESV_KW_WHILE) {
						int endNestedWhileBlock = FindEndScopeBlock(codeList.ToArray<string>(), i, "{", "}");
						i = endNestedWhileBlock;
						continue;
					}
					if(codeList[i] == PH_RESV_KW_BREAK) {
						codeList[i] = PH_RESV_KW_GOTO + PH_ID + PH_LABEL + PH_ID + "while_block_end_" + whileID;
					} else if(codeList[i] == PH_RESV_KW_CONTINUE) {
						codeList[i] = PH_RESV_KW_GOTO + PH_ID + PH_LABEL + PH_ID + "while_block_begin_" + whileID;
					}
				}

				// Let's clean the codes again by separating into tokens
				string newCodes = "";
				for(int i = 0; i < codeList.Count; i++) newCodes += codeList[i];
				string[] newCodesArray = splitCodesToArray(newCodes);
				return newCodesArray;
			}

			/// <summary>
			/// Convert labels "labelname:" to placeholder symbols, so we can distinguish between labels and class inheritences
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertLabelsToPlaceHolders(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != ":") continue;
					// Ignore if the colon does not belong to a class inheritance
					if(codes[i - 1].IndexOf(PH_RESV_KW_CLASS, 0) != -1) continue;
					// label found
					codes[i - 1] = PH_ID + PH_LABEL + PH_ID + codes[i - 1].Replace(":", "");
					codes[i] = ";";
				}
				return codes;
			}


			/// <summary>
			/// Place the equation for logical comparison inside if/else if statements outside the if/else if brackets,
			/// and also place a label for the start and end block of the if statement
			/// e.g.
			/// if(a == b) {} else if(a == c) {} else {}
			/// -->
			/// byte f = a == b; if(f) {} var s = a == c; else if(s) {} else {}
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] PreProcessIfElseBlock(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == PH_RESV_KW_IF) {
						codes = SeparateIfElseExpressionsAndAddLabel(codes, i);
						// Since position of "if" statement has been shifted as a result of SeparateIfElseExpressionsAndAddLabel, we will goto the new shifted code index
						for(int j = i; j < codes.Length; j++) {
							if(codes[j] == PH_RESV_KW_IF) {
								// Then, start searching for another if statement block after this
								i = j;
								break;
							}
						}
					}
				}
				return codes;
			}

			/// <summary>
			/// Processes the following for a single if/else if/else block:
			/// Place the equation for logical comparison inside if/else if statements outside the if/else if brackets,
			/// and also place a label for the start and end block of the if statement
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startCodeIndex"></param>
			/// <returns></returns>
			private string[] SeparateIfElseExpressionsAndAddLabel(string[] codes, int startCodeIndex) {
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);
				// First of all, let's find out the parent scope of this if-elseif-else statement block
				// e.g.
				// {  <-----------------  we need to find where the scope for this if statement begins
				//		if(...) {} else {}
				// }  <-----------------  we also need to find where the scope for this if statement ends
				// To do this, we will need to trace back the line of scopes until we reach the start of scope bracket "{"
				int startParentScopeBlock = 0;
				for(int i = startCodeIndex; i >= 0; i--) {
					if(codeList[i] == "{") {
						startParentScopeBlock = i;
						break;
					}
				}
				// Next, we need to find where the parent scope block ends
				int endParentScopeBlock = FindEndScopeBlock(codes, startParentScopeBlock, "{", "}");

				// An unique ID to be used for the if/else if/else block
				string ifID = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

				// Let's get the equation for logical comparison inside the round brackets
				int logicalComparisonEQ = FindEndScopeBlock(codes, startCodeIndex + 1, "(", ")");
				int scopeBlockEndIndex = FindEndScopeBlock(codes, logicalComparisonEQ + 1, "{", "}");
				// Create a new variable to store the logical comparison expression
				string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

				// Generate logical comparison expression outside the "if/else if" keyword				
				codeList[startCodeIndex + 1] = PH_RESV_KW_BYTE + tmpVar + "=";
				codeList[logicalComparisonEQ] = ";";
				codeList.Insert(logicalComparisonEQ + 1, PH_RESV_KW_IF + "(" + tmpVar + ")");
				codeList.RemoveAt(startCodeIndex);
				// Add beginning of if/else if scope block label
				codeList[logicalComparisonEQ - 1] = ";" + PH_ID + PH_LABEL + PH_ID + "if_block_begin_" + ifID + ";";
				codeList[logicalComparisonEQ - 1] += PH_ID + PH_LABEL + PH_ID + "if_begin_" + ifID + ";";
				// Add end label for this if statement block
				codeList[scopeBlockEndIndex] = "} " + PH_ID + PH_LABEL + PH_ID + "if_end_" + ifID + ";";

				// Increment as elseif statement is found
				int elseifCounter = 0;

				// Do we have an elseif or else statement straight after the if statement block?
				int endOfIfElseBlockIndex = -1;
				if(codes[scopeBlockEndIndex + 1] == PH_RESV_KW_ELSE) {
					// Add start of else label
					codeList[scopeBlockEndIndex] += PH_ID + PH_LABEL + PH_ID + "else_begin_" + ifID + ";";
					// If we have an else statement, this is the final block of the if/elseif/else block
					endOfIfElseBlockIndex = FindEndScopeBlock(codes, scopeBlockEndIndex + 1, "{", "}");
					// Add end label for this else and if statement block
					codeList[endOfIfElseBlockIndex] = "} " + PH_ID + PH_LABEL + PH_ID + "else_end_" + ifID + ";";
					codeList[endOfIfElseBlockIndex] += PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";";
				} else if(codes[scopeBlockEndIndex + 1] == PH_RESV_KW_ELSEIF) {

				// We found an elseif block
				else_if_block:
					startCodeIndex = scopeBlockEndIndex + 1;
					// Let's get the equation for logical comparison inside the round brackets
					logicalComparisonEQ = FindEndScopeBlock(codes, startCodeIndex + 1, "(", ")");
					int elseifScopeBlockEndIndex = FindEndScopeBlock(codes, logicalComparisonEQ + 1, "{", "}");
					// Create a new variable to store the logical comparison expression
					tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);

					// Generate logical comparison expression outside the "else if" keyword				
					codeList[startCodeIndex + 1] = PH_RESV_KW_BYTE + tmpVar + "=";
					codeList[logicalComparisonEQ] = ";";
					codeList.Insert(logicalComparisonEQ + 1, PH_RESV_KW_ELSEIF + "(" + tmpVar + ")");
					codeList.RemoveAt(startCodeIndex);
					// Add beginning of else if scope block label
					codeList[logicalComparisonEQ - 1] = ";" + PH_ID + PH_LABEL + PH_ID + "elseif_begin_" + elseifCounter + "_" + ifID + ";";
					endOfIfElseBlockIndex = FindEndScopeBlock(codes, scopeBlockEndIndex + 1, "{", "}");
					codeList[endOfIfElseBlockIndex] = "} " + PH_ID + PH_LABEL + PH_ID + "elseif_end_" + elseifCounter + "_" + ifID + ";";
					elseifCounter++;

					// Now, check for further ifelse or else statement within this scope
					if(codes[endOfIfElseBlockIndex + 1] == PH_RESV_KW_ELSE) {
						// Add start of else label
						codeList[endOfIfElseBlockIndex] += PH_ID + PH_LABEL + PH_ID + "else_begin_" + ifID + ";";
						// If we have an else statement, this is the final block of the if/elseif/else block
						endOfIfElseBlockIndex = FindEndScopeBlock(codes, endOfIfElseBlockIndex + 1, "{", "}");
						// Add end label for this else statement block
						codeList[endOfIfElseBlockIndex] = "} " + PH_ID + PH_LABEL + PH_ID + "else_end_" + ifID + ";";
						codeList[endOfIfElseBlockIndex] += PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";";
					} else if(codes[endOfIfElseBlockIndex + 1] == PH_RESV_KW_ELSEIF) {
						// Found another elseif block
						scopeBlockEndIndex = endOfIfElseBlockIndex;
						goto else_if_block;

					} else {
						// No elseif or else statement found, so we know that this ifelse statement does not have corresponding elseif/else that follows it
						codeList[endOfIfElseBlockIndex] += PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";";
					}
					for(int i = scopeBlockEndIndex + 1; i < endParentScopeBlock; i++) { }
				} else {
					// No elseif or else statement found, so we know that this if statement does not have corresponding elseif/else that follows it
					codeList[scopeBlockEndIndex] += PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";";
				}


				// Let's clean the codes again by separating into tokens
				string newCodes = "";
				for(int i = 0; i < codeList.Count; i++) newCodes += codeList[i];
				string[] newCodesArray = splitCodesToArray(newCodes);
				return newCodesArray;
			}

			/// <summary>
			/// Convert array declarations (including multi-dimentional arrays) to C language formats
			/// "ptr𒀭 a; a = .......... 𒀭PH_NEW𒀭int[2]"  --->  "ptr𒀭 a[2];"
			/// In the code of lines, we need to replace the line with the "new" keyword to contain the variable pointer
			/// [0] ptr𒀭 N.ArrayTest.x
			/// [1] ;
			/// [n] ...
			/// [10] N.ArrayTest.x
			/// [11] =
			/// [12] 𒀭PH_NEW𒀭int[2]
			/// ------->> Becomes ------->>
			/// [0] (removed)
			/// [1] (removed)
			/// [n] ...
			/// [10] ptr𒀭 N.ArrayTest.x[2]
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertArraysToCLangFormat(string[] codes) {

				return codes;
				/*
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);
				for(int i = 0; i < codeList.Count; i++) {
					if(codeList[i].IndexOf(PH_ID + PH_NEW + PH_ID) == -1) continue;
					if(codeList[i].IndexOf("[") == -1) continue;
					// The code before the array instantiation is always equal "="
					// So we need to get the code before the equal sign
					string arrayDeclareStr = codeList[i - 2];
					// If the left side of assignment operator contains "ptr𒀭 ", this is the declaration code
					if(arrayDeclareStr.IndexOf(PH_RESV_KW_PTR) != -1) {
						string arrayName = ReplaceFirstOccurance(arrayDeclareStr, PH_RESV_KW_PTR, "");
						// Get the type of array
						string arrayType = Regex.Match(codeList[i], @"(?<=" + PH_ID + PH_NEW + PH_ID + @").+?(?=\[)").Value;
						// Get the size of array

					} else {
						// If not, then it is declared somewhere else, so we will try to find the declaration statement in our code list
					}
				}
				return codeList.ToArray<string>();
				*/
			}


			/// <summary>
			/// Convert arrays (including multi-dimentional arrays) declarations to pointers
			/// e.g.
			/// int[] a;  --->  ptr a;
			/// string[,,] str = new string[1][2] --->  ptr str = new string[1][2]
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ConvertArraysToCLangPointerNotations(string[] codes) {
				// Note that one-dimentional arrays are sotred in: "int[] x" format
				// But since C# multi-dimentional arrays are declared with commas ([,,]) these are broken down into several lines of code
				// e.g. multi-dimentional arrays
				// [0] int[
				// [1] ,
				// [2] ,
				// [3] ] x
				for(int i = 0; i < codes.Length; i++) {
					if((codes[i].IndexOf("[]") != -1) && (codes[i].IndexOf(" ") != -1)) {
						// Found a one-dimentional array declaration
						string[] splitCode = codes[i].Split(' ');
						string chkVarType = splitCode[0].Replace("[]", "");
						// char[] s  --> char_ptr*
						string varType = chkVarType + "_" + PH_RESV_KW_PTR;
						// Only convert primitive types (int, char, etc) to dedicated pointer representations
						// User-Defined classes will remain as "ptr*"
						if(!AllPrimitiveTypePointerKeywords.Contains<string>(varType)) {
							varType = PH_RESV_KW_PTR;
						}
						//ActualClassMemberVarTypeList.Add(splitCode[1], splitCode[0].Replace("[]", ""));
						codes[i] = Regex.Replace(codes[i], @"(.*?)\] ", varType);
					} else if(codes[i].Substring(codes[i].Length - 1, 1) == "[") {
						// Found the start index of a multi-dimentional array declaration
						// Go down the code to get the end of declaration
						int endIndex = -1;
						for(int j = i + 1; j < codes.Length; j++) {
							if(codes[j].IndexOf("] ") != -1) {
								endIndex = j;
								// Get the variable name
								string[] splitCode = codes[j].Split(' ');
								codes[i] = Regex.Replace(codes[i], @"(.*?)\[", PH_RESV_KW_PTR + splitCode[1]);
								codes[j] = "";
								i = endIndex;
								break;
							} else {
								// Remove other codes
								codes[j] = "";
							}
						}
					}
				}
				// Get rid of empty code and trim
				codes = codes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
				return codes;
			}

			/// <summary>
			/// Merges extended parent class to the current class
			/// NOTE: The Parent class remains unmodified as it might be used as a stand alone class elsewhere
			/// e.g.
			/// class ParentTest {
			///		int a;
			///		int b;
			///		function A {}
			///		function B {}
			/// }
			/// class Test : ParentTest {
			///		int a;
			///		int c;
			///		function A {}
			/// }
			///
			/// -------> MERGE/CONVERT TO ------>
			///
			/// class Test {
			///		int ParentTest_a;  // If duplicates found, parent class name is added before the variable/function, and are treated as separate entities
			///		int a;
			///		int b;
			///		int c;  // NOTE: If a variable or function only exists in the parent class, it can be accessed by either "c" or "base.c" or "this.c" by the derived class
			///
			///		function ParentTest_A {}   // If duplicates found, parent class name is added before the variable/function, and are treated as separate entities
			///		function A {}
			///		function B {}  // NOTE: If a variable or function only exists in the parent class, it can be accessed by either "B()" or "base.B()" or "this.B()" by the derived class
			/// }
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveExtendedClass(string[] codes) {
				// Generates a list of classes, reordered in dependancy order
				string[] reorderedClassList = GenerateClassListReorderedByDependancy(codes, false);
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_GenerateClassListReorderedByDependancy.txt", string.Join("\n", reorderedClassList));
				// For each class, resolve extended classes and merge parent with derived class together
				for(int i = 0; i < reorderedClassList.Length; i++) {
					for(int j = 0; j < codes.Length; j++) {
						if(codes[j] == PH_RESV_KW_CLASS + reorderedClassList[i]) {
							codes = MergeExtendedClass(codes, j);
							// For each class, add parent constructor calls to the constructor
							codes = AddParentConstructorCalls(codes);
							break;
						}
					}
				}
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_MergeExtendedClass.txt", string.Join("\n", codes));
				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_AddParentConstructorCalls.txt", string.Join("\n", codes));
				return codes;
			}

			/// <summary>
			/// Adds constructors to static class
			/// In C#, you cannot add constructors to static classes, but we add it in so all class member initiations can be done in this constructor later on
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] AddConstructorsToStaticClass(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length != 2) continue;
					if(splitCode[0] + " " != PH_RESV_KW_STATIC_CLASS) continue;
					string[] splitClassName = splitCode[1].Split('.');
					string classNameWithoutScope = splitClassName[splitClassName.Length - 1];
					// We can safely insert a constructor at the top of class since C# compiler will not compile with already existing constructors for static classes
					codes[i + 1] = "{" + PH_RESV_KW_FUNCTION_VOID + splitCode[1] + "." + classNameWithoutScope + "(){ return(0); }";
				}
				// Clean up code
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// For each class, add parent constructor calls to the constructor
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] AddParentConstructorCalls(string[] codes) {
				// For each class, add parent constructor calls to the constructor
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);
				for(int i = 0; i < codeList.Count; i++) {
					// class extend symbol found
					if(codeList[i] != ":") continue;
					string normalizedDerivedClassName = codeList[i - 1].Replace(PH_RESV_KW_CLASS, "");
					string[] splitNormalizedDerivedClassName = normalizedDerivedClassName.Split('.');
					string actualDerivedClassName = splitNormalizedDerivedClassName[splitNormalizedDerivedClassName.Length - 1];
					string notmalizedParentClassName = codeList[i + 1];
					string[] splitParentClassName = notmalizedParentClassName.Split('.');
					string actualParentClassName = splitParentClassName[splitParentClassName.Length - 1];

					// For this class, check to see if there's a constructor
					int scopeBlockStartIndex = i - 1;
					int scopeBlockEndIndex = FindEndScopeBlock(codeList.ToArray<string>(), scopeBlockStartIndex, "{", "}");
					string constructorName = normalizedDerivedClassName + "." + actualDerivedClassName;
					for(int j = scopeBlockStartIndex; j < scopeBlockEndIndex; j++) {
						if(codeList[j] == PH_RESV_KW_FUNCTION_VOID + constructorName) {
							// Check if the constructor does not have any parameters
							// If the a constructor contains parameters, we simply ignore
							if(codeList[j + 2] != ")") continue;
							// Insert a call to the parent constructor (if it doesn't exist already)
							string callToParentConstructorCode = normalizedDerivedClassName + "." + notmalizedParentClassName + "." + actualParentClassName;
							if(codeList[j + 4] == callToParentConstructorCode) continue;
							List<string> newCode = new List<string>();
							newCode.Add(callToParentConstructorCode);
							newCode.Add("(");
							newCode.Add(")");
							newCode.Add(";");
							codeList.InsertRange(j + 4, newCode);
						}
					}
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Generates a list of classes, reordered in dependancy order
			/// e.g. If a code contains classes like:
			/// [0] Test:ParentTest
			/// [1] ParentTest:Base
			/// [2] Base
			/// -----> Will return a new list of ---->
			/// [0] Base
			/// [1] ParentTest:Base
			/// [2] Test:ParentTest
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="isRemoveNonDependantClass">Remove classes that do not get inherited</param>
			/// <returns></returns>
			private string[] GenerateClassListReorderedByDependancy(string[] codes, bool isRemoveNonDependantClass = false) {
				// First of all, check which classes we should resolve first (i.e. the upper most base class needs to be proccessed as priority)
				// Get list of class names with its parent class separated by a colon (should be normalized class names as prerequisite)
				// (NOTE: Since static classes cannot be inherited, we do not need obtain static classes)
				string[] classDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, true);
				// Bring base classes (classes without ":") to top of list
				List<string> reorderedClasses = new List<string>();
				List<string> derivedClasses = new List<string>();
				for(int i = 0; i < classDeclarationList.Length; i++) {
					if(classDeclarationList[i].IndexOf(":") == -1) {
						reorderedClasses.Add(classDeclarationList[i]);
					} else {
						derivedClasses.Add(classDeclarationList[i]);
					}
				}
				reorderedClasses.AddRange(derivedClasses);

				// For each class declarations, re-order the list orders so that the least dependant class is moved to the top of list
				for(int i = 0; i < reorderedClasses.Count; i++) {
					if(reorderedClasses[i].IndexOf(":") == -1) continue;
					string[] classSep = reorderedClasses[i].Split(':');
					string parentClassToSearchFor = classSep[1];
					for(int j = 0; j < reorderedClasses.Count; j++) {
						if(reorderedClasses[j].IndexOf(":") == -1) continue;
						string[] parentClassSep = reorderedClasses[j].Split(':');
						if(parentClassSep[0] == parentClassToSearchFor) {
							// Check if parent class exists before the current derived class
							if(j < i) break;
							// If not, move the parent class one index above the derived class
							string parentClassDef = reorderedClasses[j];
							reorderedClasses.RemoveAt(j);
							reorderedClasses.Insert(i, parentClassDef);
						}
					}
				}

				// For each reordered class list, remove the parent class
				for(int i = 0; i < reorderedClasses.Count; i++) {
					int colonIndex = reorderedClasses[i].IndexOf(":");
					// Remove classes that do not get inherited
					if(isRemoveNonDependantClass) {
						if(colonIndex == -1) reorderedClasses[i] = "";
					}
					if(colonIndex != -1) reorderedClasses[i] = reorderedClasses[i].Substring(0, colonIndex);
				}

				// Remove any blank array list items
				reorderedClasses = reorderedClasses.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

				return reorderedClasses.ToArray<string>();
			}

			private string[] AddConstructorToDerivedClass(List<string> codeList, int startClassDefIndex, int derivedClassStartIndex, string derivedClassName, string parentClassName) {
				// Generate class member list (variables and functions) for derived class
				Dictionary<int, string> derivedClassMemberList = GenerateScopeMemberList(codeList.ToArray<string>(), derivedClassStartIndex);
				// In the derived class, check to see if we have a constructor
				string[] splitDerivedClassName = derivedClassName.Split('.');
				string actualDerivedClassName = splitDerivedClassName[splitDerivedClassName.Length - 1];
				string derivedClassConstructorCode = PH_RESV_KW_FUNCTION_VOID + derivedClassName + "." + actualDerivedClassName;
				bool isDerivedClassConstructorExists = false;
				for(int k = 0; k < derivedClassMemberList.Count; k++) {
					if(derivedClassMemberList.ElementAt(k).Value == derivedClassConstructorCode) {
						// Check to see if this constructor does not have any parameters
						if(codeList[derivedClassMemberList.ElementAt(k).Key + 2] == ")") {
							isDerivedClassConstructorExists = true;
							break;
						}
					}
				}
				List<string> newConstructorCodeList = new List<string>();
				// If parent class does not have a constructor, then we will add an empty parent class constructor to the parent class
				if(!isDerivedClassConstructorExists) {
					newConstructorCodeList.Add(derivedClassConstructorCode);
					newConstructorCodeList.Add("(");
					newConstructorCodeList.Add(")");
					newConstructorCodeList.Add("{");
					if(parentClassName != "") {
						string[] splitParentClassName = parentClassName.Split('.');
						string actualParentClassName = splitParentClassName[splitParentClassName.Length - 1];
						newConstructorCodeList.Add(derivedClassName + "." + parentClassName + "." + actualParentClassName);
						newConstructorCodeList.Add("(");
						newConstructorCodeList.Add(")");
						newConstructorCodeList.Add(";");
					}
					newConstructorCodeList.Add("return");
					newConstructorCodeList.Add("(");
					newConstructorCodeList.Add("0");
					newConstructorCodeList.Add(")");
					newConstructorCodeList.Add(";");

					newConstructorCodeList.Add("}");

					int insertIndex = startClassDefIndex + 3;
					if(parentClassName == "") {
						insertIndex = startClassDefIndex + 2;
					}
					codeList.InsertRange(insertIndex, newConstructorCodeList);
				}
				return codeList.ToArray<string>();
			}

			/// <summary>
			/// Merges parent class with derived class
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startClassDefIndex">The line index in the line of codes where the class is defined</param>
			/// <returns></returns>
			private string[] MergeExtendedClass(string[] codes, int startClassDefIndex) {
				List<string> codeList = new List<string>();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);

				int derivedClassStartIndex = 0;

				// This is the code index where class extend symbol is found
				if(codeList[startClassDefIndex + 1] == ":") {
					startClassDefIndex++;
					// If colon is found, check if it is a class (NOT a static class --> Classes cannot be derived from static classes)
					if(codeList[startClassDefIndex - 1].IndexOf(PH_RESV_KW_CLASS, 0) == -1) return codes;
					derivedClassStartIndex = startClassDefIndex - 1;
				} else {
					// If colon is found, check if it is a class (NOT a static class --> Classes cannot be derived from static classes)
					if(codeList[startClassDefIndex].IndexOf(PH_RESV_KW_CLASS, 0) == -1) return codes;
					derivedClassStartIndex = startClassDefIndex;
					return AddConstructorToDerivedClass(codeList, startClassDefIndex, startClassDefIndex, codeList[derivedClassStartIndex].Replace(PH_RESV_KW_CLASS, ""), "");
				}


				string derivedClassName = codeList[derivedClassStartIndex].Replace(PH_RESV_KW_CLASS, "");
				string parentClassName = codeList[startClassDefIndex + 1];
				// Generate class member list (variables and functions) for derived class
				Dictionary<int, string> derivedClassMemberList = GenerateScopeMemberList(codes, derivedClassStartIndex);
				// Generate class member list (variables and functions) for parent class
				Dictionary<int, string> parentClassMemberList = GenerateScopeMemberList(codes, 0, PH_RESV_KW_CLASS + parentClassName);

				// Do we have the parent class? Does it really exist?
				bool isParentClassExist = false;
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == PH_RESV_KW_CLASS + parentClassName) {
						isParentClassExist = true;
						break;
					}
				}
				if(!isParentClassExist) return codes;

				/*
				// This is the code index where class extend symbol is found
				if(codeList[startClassDefIndex] != ":") {
					derivedClassStartIndex = startClassDefIndex;
					derivedClassName = codeList[derivedClassStartIndex].Replace(PH_RESV_KW_CLASS, "");
					return AddConstructorToDerivedClass(codeList, startClassDefIndex, startClassDefIndex, derivedClassName, "");
				}
				*/

				// Check if we have any duplicate var/func members
				for(int j = 0; j < parentClassMemberList.Count; j++) {
					// Flag to determine if the member contains scope brackets "{" and "}"
					bool isScopeBlock = false;
					string[] declareKeyword = parentClassMemberList.ElementAt(j).Value.Split(' ');
					if(NonVariableDeclareKeywords.Contains<string>(declareKeyword[0] + " ")) isScopeBlock = true;
					int endMemberCodeStart = -1;
					int startMemberCodeStart = parentClassMemberList.ElementAt(j).Key;
					if(isScopeBlock) {
						endMemberCodeStart = FindEndScopeBlock(codes, startMemberCodeStart, "{", "}");
					} else {
						endMemberCodeStart = FindEndScopeBlock(codes, startMemberCodeStart, "", ";");
					}

					List<string> newCodeList = new List<string>();

					// Check if we have any duplicate var/func members
					bool isSameMemberNameExists = false;
					string parentMemberToCopy = codes[startMemberCodeStart];
					string[] parentMemberToCopySplit = parentMemberToCopy.Split('.');
					string parentMemberName = parentMemberToCopySplit[parentMemberToCopySplit.Length - 1];
					for(int k = 0; k < derivedClassMemberList.Count; k++) {
						string[] derivedClassMemberNameSplit = derivedClassMemberList.ElementAt(k).Value.Split('.');
						string derivedClassMemberName = derivedClassMemberNameSplit[derivedClassMemberNameSplit.Length - 1];
						if(derivedClassMemberName == parentMemberName) {
							isSameMemberNameExists = true;
							break;
						}
					}

					/*
					// Check if we have any duplicate var/func members
					bool isSameMemberNameExists = false;
					for(int k = 0; k < derivedClassMemberList.Count; k++) {
						string[] derivedClassMemberNameSplit = derivedClassMemberList.ElementAt(k).Value.Split('.');
						string derivedClassMemberName = derivedClassMemberNameSplit[derivedClassMemberNameSplit.Length - 1];
						for(int m = 0; m < parentClassMemberList.Count; m++) {
							string[] parentClassMemberNameSplit = parentClassMemberList.ElementAt(m).Value.Split('.');
							string parentClassMemberName = parentClassMemberNameSplit[parentClassMemberNameSplit.Length - 1];
							if(derivedClassMemberName == parentClassMemberName) {
								isSameMemberNameExists = true;
								break;
							}
						}
						if(isSameMemberNameExists) break;
					}*/

					// Check for duplicated member names in parent/derived class
					if(isSameMemberNameExists) {
						// Duplicate name members found, so we will add the duplicate of the same base class member, with the derived classe's namespace added on top of the base class member name
						// e.g. Base.A()  -->  Derived.Base.A()
						for(int k = startMemberCodeStart; k <= endMemberCodeStart; k++) {
							newCodeList.Add(ReplaceFirstOccurance(codes[k], parentClassName, derivedClassName + "." + parentClassName));
						}
					} else {
						// Duplicate member does not exist, so we will copy the base classe's member pretending as if it is the derived classe's member
						// e.g. Base.A()  -->  Derived.A()
						for(int k = startMemberCodeStart; k <= endMemberCodeStart; k++) {
							newCodeList.Add(ReplaceFirstOccurance(codes[k], parentClassName, derivedClassName));
						}
						// And also add the duplicate of the same base class member, but this time with the derived classe's namespace added on top of the base class member name
						// e.g. Base.A()  -->  Derived.Base.A()
						for(int k = startMemberCodeStart; k <= endMemberCodeStart; k++) {
							newCodeList.Add(ReplaceFirstOccurance(codes[k], parentClassName, derivedClassName + "." + parentClassName));
						}
					}

					codeList.InsertRange(startClassDefIndex + 3, newCodeList);
				}

				codes = codeList.ToArray<string>();

				// Index might have changed because we've inserted new code, so re-analyze to get the correct member code indexes again
				codeList.Clear();
				for(int i = 0; i < codes.Length; i++) codeList.Add(codes[i]);

				string[] ret = AddConstructorToDerivedClass(codeList, startClassDefIndex, startClassDefIndex, derivedClassName, parentClassName);
				return ret;
				/*
				// Generate class member list (variables and functions) for derived class
				derivedClassMemberList = GenerateScopeMemberList(codes, derivedClassStartIndex);
				// In the derived class, check to see if we have a constructor
				string[] splitDerivedClassName = derivedClassName.Split('.');
				string actualDerivedClassName = splitDerivedClassName[splitDerivedClassName.Length - 1];
				string derivedClassConstructorCode = PH_RESV_KW_FUNCTION + derivedClassName + "." + actualDerivedClassName;
				bool isDerivedClassConstructorExists = false;
				for(int k = 0; k < derivedClassMemberList.Count; k++) {
					if(derivedClassMemberList.ElementAt(k).Value == derivedClassConstructorCode) {
						// Check to see if this constructor does not have any parameters
						if(codeList[derivedClassMemberList.ElementAt(k).Key + 2] == ")") {
							isDerivedClassConstructorExists = true;
							break;
						}
					}
				}
				List<string> newConstructorCodeList = new List<string>();
				// If parent class does not have a constructor, then we will add an empty parent class constructor to the parent class
				if(!isDerivedClassConstructorExists) {
					newConstructorCodeList.Add(derivedClassConstructorCode);
					newConstructorCodeList.Add("(");
					newConstructorCodeList.Add(")");
					newConstructorCodeList.Add("{");
					string[] splitParentClassName = parentClassName.Split('.');
					string actualParentClassName = splitParentClassName[splitParentClassName.Length - 1];
					newConstructorCodeList.Add(derivedClassName + "." + parentClassName + "." + actualParentClassName);
					newConstructorCodeList.Add("(");
					newConstructorCodeList.Add(")");
					newConstructorCodeList.Add(";");
					newConstructorCodeList.Add("}");
					codeList.InsertRange(startClassDefIndex + 3, newConstructorCodeList);
				}

				return codeList.ToArray<string>();
				*/
			}


			/// <summary>
			/// Generates list of members defined within the specified scope (i.e. member list inside a class {...})
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="startCodeIndex"></param>
			/// <param name="scopeName"></param>
			/// <returns></returns>
			private Dictionary<int, string> GenerateScopeMemberList(string[] codes, int startCodeIndex, string scopeName = "") {
				// Get all class names
				string[] classList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);
				for(int i = 0; i < classList.Length; i++) classList[i] += " ";
				// Generate merged class member types (variable types and function types)
				List<string> mergedClassMemberTypeList = ClassMemberReservedKeyWords.ToList<string>();
				mergedClassMemberTypeList.AddRange(classList.ToList<string>());
				Dictionary<int, string> retList = new Dictionary<int, string>();
				// Search for the scope name first if scope name to search was specified (i.e. "class xxx", "function yyy")
				if(scopeName != "") {
					for(int i = 0; i < codes.Length; i++) {
						if(codes[i] == scopeName) {
							startCodeIndex = i;
							break;
						}
					}
				}
				// Get end of scope block code index
				int endScopeBlockIndex = FindEndScopeBlock(codes, startCodeIndex, "{", "}");
				for(int i = startCodeIndex; i < endScopeBlockIndex; i++) {
					string[] keywordToCheck = codes[i].Split(' ');
					if(keywordToCheck.Length < 2) continue;
					if(!mergedClassMemberTypeList.Contains<string>(keywordToCheck[0] + " ")) continue;
					// If function found, we ignore the entire function block

					if(AllFunctionKeywords.Contains<string>(keywordToCheck[0] + " ")) {

						//if(keywordToCheck[0] + " " == PH_RESV_KW_FUNCTION) {
						int endFunctionBlockIndex = FindEndScopeBlock(codes, i, "{", "}");
						retList.Add(i, codes[i]);
						i = endFunctionBlockIndex;
						continue;
					}
					retList.Add(i, codes[i]);
				}
				return retList;
			}

			/// <summary>
			/// Extract function call parameters and put the expressions outside of the parameter round brackets
			/// e.g.
			/// xxx = myfunc(1 + 2, fcall(yyy));
			/// -->
			/// var dummy1 = 1 + 2;
			/// var dummy2 = fcall(yyy);
			/// xxx = myfunc(dummy1, dummy2);
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			public string[] PreProcessFunctionParams(string[] codes) {
				//string[] ignoreOpearatorList = new string[] { "<=", ">=", "==" };
				//string[] assignmentOperatorList = new string[] { "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=" };
				for(int i = 0; i < codes.Length; i++) {
					// If we find an equal sign "=", check if it is an assignment operator or not (just check if it is not a logical operator)
					if(codes[i].IndexOf("=") != -1) {
						// Check if not a logical operator
						if(!Regex.IsMatch(codes[i - 1], @"[<|>|!]") && codes[i + 1] != "=") {
							// Check if we are dealing with function calls
							// i.e. If an equal sign is found, then the next index is the function name, and the one after is the round bracket
							// [3] =         <------- we are currently on this index
							// [4] MyFunction
							// [5] (         <------- indicates a function call
							if(codes[i + 2] != "(") continue;
							// If there are no parameters, ignore
							if(codes[i + 3] == ")") continue;
							// If a function call "DScript.VMC.__iasm" was found, we ignore this special function call which we want to process differently
							if(codes[i + 1] == PH_RESV_KW_IASM_FUNCTION_CALL) continue;

							// Trace back until the last statement ";" or "}" or "{"
							int expressionBeginIndex = 0;
							for(int j = i; j >= 0; j--) {
								if(Regex.IsMatch(codes[j], @"[;|\}|\{]") || codes[j] == "") {
									expressionBeginIndex = j + 1;
									// Just check if we have temporary variables for the function call, and if so get the declaration of the temporary variable (if exists)
									// i.e.
									// [0] byte xxxRQ20131212DSRNDID
									// [1] ;      <-------   We're here (for "j") right now
									// [2] xxxRQ20131212DSRNDID
									// [3] =
									// [4] MyFunction
									// [5] (
									// ...
									if(codes[j] == ";" && codes[j - 1].IndexOf(codes[expressionBeginIndex]) != -1 && codes[j - 1].IndexOf(TmpVarIdentifier) != -1) {
										// Start the expression begin index from the temporary var declaration code
										expressionBeginIndex = j - 1;
									}
									break;
								}
							}
							int expressionEndIndex = 0;
							int roundBracketCnt = 0;
							int funcRoundBracketStart = i + 2;
							int funcRoundBracketEnd = 0;
							// Get the end of expression
							for(int j = i; j < codes.Length; j++) {
								// If we've found a round bracket, it indicates that there's a function call inside the expression
								// And the function calls have semicolons automatically inserted during previous conversions,
								// So, we need to ignore the semicolons that are part of the round brackets
								if(codes[j] == "(") {
									roundBracketCnt++;
									continue;
								}
								if(codes[j] == ")") {
									roundBracketCnt--;
									if(roundBracketCnt == 0) funcRoundBracketEnd = j;
									continue;
								}
								if(codes[j] == ";" && roundBracketCnt == 0) {
									expressionEndIndex = j - 1;
									break;
								}
							}
							string expression = "";
							string parameters = "";
							for(int j = expressionBeginIndex; j <= expressionEndIndex; j++) {
								expression += codes[j];
								if(j > funcRoundBracketStart && j < funcRoundBracketEnd) {
									parameters += codes[j];
									// Remove all parameter expressions
									codes[j] = "";
								}
							}
							// Stores the normalized parameter as individual statements
							string normalizedParamCode = "";
							string tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier);
							// Replace commas inside brackets to placeholders first, so we can split each parameters easily
							// e.g. add(1 + 2, func(33, 66))  ---->   add(1 + 2, func(33PLACEHOLDER 66))
							string convertedParameters = Regex.Replace(parameters, @",(?=((?!\().)*?\))", tmpVar, RegexOptions.Singleline);
							// Now, split parameters inside the round brackets
							string[] splitParams = convertedParameters.Split(',');
							for(int j = 0; j < splitParams.Length; j++) {
								// Replace placeholder to normal comma back again
								splitParams[j] = splitParams[j].Replace(tmpVar, ",");
								// If the single parameter contains an equal sign, it means assignment expression is already inside the parameter
								// e.g. It might look something like:
								// "byte upszmpvpRQ20131212DSRNDID;upszmpvpRQ20131212DSRNDID=Main.TEST(33hgeseqrxRQ20131212DSRNDID66)"
								// "int joqzymfjRQ20131212DSRNDID;joqzymfjRQ20131212DSRNDID=ANKISUA.Main.KADONO(33,66);"
								if(splitParams[j].IndexOf("=") != -1) {
									// If we already have an equal sign, it's already an assignment expression,
									// so we take the first statements variable as the one to substitute as the parameter
									splitParams[j] = splitParams[j] + ";";
								} else {
									// If there are no assignment expressions, we make this parameter a separate assignment expression
									// e.g. add(1 + 2, ....)  ---->   "var xxxx = 1 + 2;"
									splitParams[j] = PH_RESV_KW_VAR + tmpVar + j.ToString() + "=" + splitParams[j] + ";";
								}
								// Stores the normalized parameter as individual statements
								normalizedParamCode += splitParams[j];
								// Get the variable name that is declared
								string declaredVarName = Regex.Match(splitParams[j], @" [a-zA-Z0-9_](.*?)(?=[^a-zA-Z0-9_\.])").Value.ToString().Trim();
								// Set the declared var to the parameter, separated by comma
								codes[funcRoundBracketStart + 1] += declaredVarName + ((j != splitParams.Length - 1) ? "," : "");

							}
							// Move the variable declaration BEFORE the function call
							codes[expressionBeginIndex] = normalizedParamCode + codes[expressionBeginIndex];
							// Move search to the end of function call statement
							i = funcRoundBracketEnd + 1;
						}
					}
				}
				// Remove any blank array list items
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// Convert expressions so that we can convert the expression into Reverse Polish Notation (RPN)
			/// 
			/// e.g.
			/// namespace N {
			///		class Exp {
			///			public Exp() {
			///				int a = 1;
			///				int result = (add(add(1, 2), 3) * (a - 5));  <---- Convert this to RPN
			///			}
			///			public int add(int a, int b) {
			///				return a + b;
			///			}
			///		}
			/// }
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			public string[] PreProcessExpressions(string[] codes) {
				//string[] ignoreOpearatorList = new string[] { "<=", ">=", "==" };
				//string[] assignmentOperatorList = new string[] { "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=" };
				for(int i = 0; i < codes.Length; i++) {
					// If we find an equal sign "=", check if it is an assignment operator or not (just check if it is not a logical operator)
					if(codes[i].IndexOf("=") != -1) {
						// Check if not a logical operator
						if(!Regex.IsMatch(codes[i - 1], @"[<|>|!]") && codes[i + 1] != "=") {
							// Trace back until the last statement ";" or "}"
							int expressionBeginIndex = 0;
							for(int j = i; j >= 0; j--) {
								if(Regex.IsMatch(codes[j], @"[;|\}|\{]") || codes[j] == "") {
									expressionBeginIndex = j + 1;
									break;
								}
							}
							int expressionEndIndex = 0;
							int roundBracketCnt = 0;
							// Get the end of expression
							for(int j = i; j < codes.Length; j++) {
								// If we've found a round bracket, it indicates that there's a function call inside the expression
								// And the function calls have semicolons automatically inserted during previous conversions,
								// So, we need to ignore the semicolons that are part of the round brackets
								if(codes[j] == "(") {
									roundBracketCnt++;
									continue;
								}
								if(codes[j] == ")") {
									roundBracketCnt--;
									continue;
								}
								if(codes[j] == ";" && roundBracketCnt == 0) {
									expressionEndIndex = j - 1;
									break;
								}
							}
							string expression = "";
							for(int j = expressionBeginIndex; j <= expressionEndIndex; j++) {
								expression += codes[j];
							}
							int typeDeclareEndIndex = expression.IndexOf(" ");
							string typeDeclareStr = "";
							if(typeDeclareEndIndex != -1) {
								typeDeclareStr = expression.Substring(0, typeDeclareEndIndex + 1);
								expression = expression.Substring(typeDeclareEndIndex + 1);
							}
							string processedExpression = BreakDownExpression(expression, codes);
							// Separate variable declaration and assignemnt
							if(typeDeclareStr != "") {
								codes[expressionBeginIndex] = codes[expressionBeginIndex] + ";" + processedExpression;
							} else {
								codes[expressionBeginIndex] = processedExpression;
							}
							for(int j = expressionBeginIndex + 1; j <= expressionEndIndex + 1; j++) codes[j] = "";
						}
					}
				}
				// Remove any blank array list items
				codes = codes.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray<string>();
				// Let's clean the codes again by separating into tokens
				codes = RegenerateCleanCode(codes);
				return codes;
			}

			/// <summary>
			/// Merges all line of codes together and make it a single string
			/// </summary>
			/// <param name="codes">String array of LOC</param>
			/// <returns>Merged code string</returns>
			public string MergeLineOfCodes(string[] codes) {
				string code = "";
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] == "{" || codes[i] == "}" || codes[i] == ";") {
						code += codes[i] + Environment.NewLine;
					} else {
						code += codes[i];
					}
				}
				return code;
			}

			/// <summary>
			/// Separate codes by tokens to make code of lines, and eliminates any empty codes
			/// </summary>
			/// <param name="code">The entire string of code</param>
			/// <returns>Separated codes by token, in a string array format</returns>
			private string[] splitCodesToArray(string code) {
				// Separate codes by tokens to make code of lines
				string[] splitCodes = Regex.Split(code, SeparatorPattern, RegexOptions.Singleline).Where((s, i) => s != "").ToArray();
				// Get rid of empty code and trim
				splitCodes = splitCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
				return splitCodes;
			}

			// Apply further optimizations for pre-processing code seaparations
			public string SanitizeKeywordForSeparation(string code) {
				code = Regex.Replace(code, PH_RESV_KW_RETURN + "(.*?);", PH_RESV_KW_RETURN + "($1);");
				return code;
			}

			/// <summary>
			/// Cleans up string by removing sequential spaces, tabs, linebreaks and single line/block comments // /* */
			/// </summary>
			/// <param name="text">Context string</param>
			/// <returns>Cleaned string</returns>
			public string CleanupCode(string text) {
				var blockComments = @"/\*(.*?)\*/";
				var lineComments = @"//(.*?)\r?\n";
				var strings = @"""((\\[^\n]|[^""\n])*)""";
				var verbatimStrings = @"@(""[^""]*"")+";

				// We need to insert new line at the end to avoid line comments not getting removed
				text = text + Environment.NewLine;

				text = Regex.Replace(text,
					blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, me => {
						if(me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
							return me.Value.StartsWith("//") ? Environment.NewLine : "";
						// Keep the literal strings
						return me.Value;
					}, RegexOptions.Singleline);
				text = text.Replace("\t", " ");
				text = text.Replace("\r\n", "");
				text = text.Replace("\r", "");
				text = text.Replace("\n", "");

				return text;
			}
			/// <summary>
			/// Converts literals to sequence of placeholders.
			/// e.g.
			/// string a = "test";
			/// string b = "code";
			/// var c = 'a'
			/// -->
			/// string a = 𒀭PH_STR0𒀭;
			/// string b = 𒀭PH_STR1𒀭;
			/// var c = 𒀭PH_CHAR0𒀭;
			/// </summary>
			/// <param name="code">The entire program code string</param>
			/// <returns>Converted code string</returns>
			public string ConvertLiteralsToPlaceholders(string code) {
				MatchCollection mcStr = new Regex("\"(.*?)\"").Matches(code);
				for(int i = 0; i < mcStr.Count; i++) {
					string strValue = mcStr[i].Value;
					code = ReplaceFirstOccurance(code, mcStr[i].Value, PH_ID + PH_STR + i + PH_ID);
					// Convert placeholders back to its equivalent symbols for storing into our list
					strValue = strValue.Replace(PH_ESC_DBL_QUOTE, "\"");
					strValue = strValue.Replace(PH_ESC_SNGL_QUOTE, "'");
					StringLiteralList.Add(PH_ID + PH_STR + i + PH_ID, strValue);
				}
				MatchCollection mcChar = new Regex("'(.*?)'").Matches(code);
				for(int i = 0; i < mcChar.Count; i++) {
					string strValue = mcChar[i].Value;
					code = ReplaceFirstOccurance(code, mcChar[i].Value, PH_ID + PH_CHAR + i + PH_ID);
					// Convert placeholders back to its equivalent symbols for storing into our list
					strValue = strValue.Replace(PH_ESC_DBL_QUOTE, "\"");
					strValue = strValue.Replace(PH_ESC_SNGL_QUOTE, "'");
					CharLiteralList.Add(PH_ID + PH_CHAR + i + PH_ID, strValue);
				}
				return code;
			}

			/// <summary>
			/// Convert "new" keywords to "new_"
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			private string ConvertNewKeywordToPlaceholders(string code) {
				code = Regex.Replace(code, @"(?<=[^a-zA-Z0-9_])new ", PH_ID + PH_NEW + PH_ID);
				return code;
			}

			/// <summary>
			/// Remove obsolete keywords that are only used for compatibility reasons with other languages
			/// </summary>
			/// <param name="code"></param>
			/// <param name="keywordList"></param>
			/// <returns></returns>
			public string ReplaceKeywords(string code, string[] keywordList) {
				// Replace "static class" to "static_class"
				code = Regex.Replace(code, PH_RESV_KW_STATIC + PH_RESV_KW_CLASS, PH_RESV_KW_STATIC_CLASS);
				// Replace "(float)" and "(double)" to the amount we need to multiply to get the whole number out of decimal point numbers
				code = code.Replace("(float)", DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY + "*");
				code = code.Replace("(double)", DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY + "*");
				// Replace other obsolete keyword as blank
				foreach(string s in keywordList) code = code.Replace(s, "");
				return code;
			}

			/// <summary>
			/// Removes C# method declaration and converts to PHP type declarations:
			/// "void A()" --> "function_void A()"
			/// "bool S(int a, int b)" --> "function_bool S(int a, int b)"
			/// _______________ TO BE IMPLEMENTED: _______________
			/// "void A()" --> "function void_A()"
			/// "bool S(int a, int b)" --> "function bool_S_a_b(int a, int b)"
			/// NOTE: C# constructors also needs to be converted ("public Game()" --> "function_void Game()")
			/// </summary>
			/// <param name="codes">The string array of tokenized code</param>
			/// <returns>String code array with Methods converted to Functions</returns>
			public string[] ConvertMethodToFunctions(string[] codes) {
				// Get all class names
				string[] classList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false);

				// All methods assumed to have round brackets after the method declaration, and is directly followed by scope brackets "{"
				// Hence, we will search for ending round brackets ")" that has immediate appearance of starting scope bracket "{"
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i] != ")") continue;
					if(codes[i + 1] == "{") {
						int bracketCnt = 1;
						// Found a method, so trace back until we find the opening round bracket "("
						for(int j = i - 1; j >= 0; j--) {
							// If we've found another "(", then we need to ignore it until we find the correct bracket pairs
							if(codes[j] == ")") {
								bracketCnt++;
							} else if(codes[j] == "(") {
								bracketCnt--;
							}
							if(codes[j] == "(" && bracketCnt == 0) {
								// If this is not a method (like if and while statements), ignore
								if(NonMethodKeywords.Contains<string>(codes[j - 1])) break;
								// Get return type of this function
								string[] split = codes[j - 1].Split(' ');
								string funcReturnType = split[0];
								// If it is a constructor (i.e. defined as "public XXXX" in the code), it will have NO return types
								if(split.Length == 1) {
									// If type does not exist, it means this function is a constructor
									funcReturnType = PH_RESV_KW_FUNCTION_VOID.Trim();
								} else if(classList.Contains<string>(funcReturnType)) {
									// It's a class type
									funcReturnType = PH_RESV_KW_FUNCTION_PTR.Trim();
								} else if(funcReturnType.IndexOf(PH_RESV_KW_PTR.Trim()) != -1) {
									// Return type may be a primitive type array e.g. "char_ptr𒀭 FuncName() {...}"
									funcReturnType = PH_RESV_KW_FUNCTION_PTR.Trim();
								} else {
									//funcReturnType += "@";
									funcReturnType = PH_RESV_KW_FUNCTION.Trim() + "_" + funcReturnType;
								}
								// Check if parameter exists, and if it does, we will add the parameter types as part of this function name
								// This is to distinguish between multiple function with same name but with different parameters
								int roundBracketEndIndex = FindEndScopeBlock(codes, j, "(", ")");
								string paramMerged = "";
								for(int k = j + 1; k < roundBracketEndIndex; k++) {
									if(codes[k] == ",") continue;
									string[] paramSplit = codes[k].Split(' ');
									paramMerged = "@" + paramSplit[0] + paramMerged;
								}

								// TODO: If we are going to allow multiple function names, uncomment & use the following code
								//codes[j - 1] = PH_RESV_KW_FUNCTION + funcReturnType + split[split.Length - 1] + paramMerged;

								// Add return types for functions (i.e. "int abc()" --> "function_int abc()"
								codes[j - 1] = funcReturnType + " " + split[split.Length - 1];

								break;
							}
						}
					}
				}
				return codes;
			}

			/// <summary>
			/// Returns the index of a string array where the end block appears
			/// </summary>
			/// <param name="codes">The string array of tokenized code</param>
			/// <param name="scopeBlockBeginIndex">Beginning of the code block index where the open scope begins (i.e. index of "{") </param>
			/// <param name="openingScopeBlock">The keyword which indicates the opening of a block (e.g. "{", "(", "[")</param>
			/// <param name="endingScopeBlock">The keyword which indicates the opening of a block (e.g. "}", ")", "]")</param>
			/// <returns>The index where the end scope block (i.e. "}") of the scope block is found, else returns -1 if not found</returns>
			public int FindEndScopeBlock(string[] codes, int scopeBlockBeginIndex, string openingScopeBlock, string endingScopeBlock) {
				int endIndex = -1;
				int scopeKeywordCounter = 0;
				if(openingScopeBlock == "") scopeKeywordCounter = 1;
				for(int i = scopeBlockBeginIndex; i < codes.Length; i++) {
					if(codes[i] == openingScopeBlock) scopeKeywordCounter++;
					if(codes[i] == endingScopeBlock) {
						scopeKeywordCounter--;
						if(scopeKeywordCounter == 0) {
							endIndex = i;
							break;
						}
					}
				}
				return endIndex;
			}

			/// <summary>
			/// Get the list of class names
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] GetAllKeywordDeclarations(string[] codes, string keyword, bool isObtainParentClass = false) {
				List<string> classNameList = new List<string>();
				foreach(int scopeBlockBeginIndex in GenerateKeywordIndexList(codes, keyword)) {
					string normalizedClassName = codes[scopeBlockBeginIndex].Substring(keyword.Length);
					// Only store unique names
					if(!classNameList.Contains(normalizedClassName)) {
						string parentClassName = "";
						if(isObtainParentClass) {
							// Parent class found
							if(codes[scopeBlockBeginIndex + 1] == ":") {
								parentClassName = ":" + codes[scopeBlockBeginIndex + 2];
							}
						}
						classNameList.Add(normalizedClassName + parentClassName);
					}
				}
				return classNameList.ToArray<string>();
			}

			/// <summary>
			/// Join the reserved keyword list with the class names
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] GenerateDefinedTypeList(string[] codes) {
				// To normalize class objects, first, get all the class names, and we will make this the searching keyword
				string[] classNameList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS);

				/*
				// We treat the class name as one type of reserved keywords and normalize all object-type declarations within scope
				// There's also a possibility where class names contain namespaces
				// e.g.
				// namespace N { class C {
				// --> N.C
				// So we generate a normalized class name and then store the normalized names in the list as well
				string[] tmpCodes = new string[codes.Length];
				for(int i = 0; i < codes.Length; i++) tmpCodes[i] = codes[i];
				// Normalize namespace's classes
				foreach(int index in GenerateKeywordIndexList(tmpCodes, PH_RESV_KW_NAMESPACE)) {
					tmpCodes = AddKeywordScope(tmpCodes, index, PH_RESV_KW_NAMESPACE, PH_RESV_KW_CLASS, "{", "}");
				}
				if(isPreNormalizeClassNames) {
					// For each normalized class name, add to list
					foreach(int scopeBlockBeginIndex in GenerateKeywordIndexList(tmpCodes, PH_RESV_KW_CLASS)) {
						string normalizedClassName = ReplaceFirstOccurance(tmpCodes[scopeBlockBeginIndex], PH_RESV_KW_CLASS, "");
						// Only store unique class names
						if(!classNameList.Contains(normalizedClassName)) classNameList.Add(normalizedClassName);
					}
				}*/
				// Merge all keywords together
				string[] keywordList = new string[ClassMemberReservedKeyWords.Length + classNameList.Length];
				int di = 0;
				foreach(string s in ClassMemberReservedKeyWords) keywordList[di++] = s;
				// Add the class names as keywords
				// Also, add extra blank spaces to each class name
				foreach(string s in classNameList) {
					keywordList[di++] = s + " ";
				}

				return keywordList;
			}

			/// <summary>
			/// Add namespaces for class type definitions
			/// e.g.
			/// CLASSNAME c = new CLASSNAME();
			/// -->
			/// NS.CLASSNAME c = new CLASSNAME();
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveClassTypeDefinitionScopes(string[] codes, bool isResolveFloatingObject = false) {
				// Get all names declared using the "using" keyword
				string[] usingNameList = AllUsingNamespaceDeclarations.ToArray<string>();
				// Get list of class names (should be normalized class names as prerequisite)
				string[] classDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS);

				// Get list of static class names (should be normalized class names as prerequisite)
				string[] staticClassDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_STATIC_CLASS);

				// Merge normal class & static classes together
				Array.Resize(ref classDeclarationList, classDeclarationList.Length + staticClassDeclarationList.Length);
				Array.Copy(staticClassDeclarationList, 0, classDeclarationList, classDeclarationList.Length - staticClassDeclarationList.Length, staticClassDeclarationList.Length);

				List<string> nameSpaceList = new List<string>();

				// For each instantiation code found..
				for(int i = 0; i < codes.Length; i++) {
					// Store the name spaces as we go through our code
					if(codes[i].IndexOf(PH_RESV_KW_NAMESPACE, 0) != -1) {
						nameSpaceList.Add(codes[i].Substring(PH_RESV_KW_NAMESPACE.Length));
					}

					string[] split = codes[i].Split(' ');
					string currentClassName = "";
					if(codes[i].IndexOf(PH_ID + PH_NEW + PH_ID) != -1) {
						currentClassName = codes[i].Replace(PH_ID + PH_NEW + PH_ID, "");
					} else {
						if(!isResolveFloatingObject && split.Length < 2) continue;
						currentClassName = split[0];
					}
					// If the class name contains a dot ".", it indicates that the namespace is already assigned to the class
					if(currentClassName.IndexOf(".") != -1) {
						// Check if the current classname containing a dot "." also contains a valid namespace at the beginning
						// If it does, then we can ignore adding namespaces to this class name
						string[] chkSpecifiedScopeStr = currentClassName.Split('.');
						if(usingNameList.Contains<string>(chkSpecifiedScopeStr[0])) continue;
						// Else, just remove the last string separated by the dot "."
						string exFuncVarName = "";
						for(int j = 0; j < chkSpecifiedScopeStr.Length - 1; j++) {
							exFuncVarName += chkSpecifiedScopeStr[j] + ".";
						}
						currentClassName = exFuncVarName.Substring(0, exFuncVarName.Length - 1);
					}
					if(!Regex.IsMatch(currentClassName, @"[a-zA-Z_" + PH_ID + "]")) continue;

					// Ignore elementary defined variable types (namespace, class, int, string, float, etc...)
					if(AllReservedKeywords.Contains<string>(currentClassName + " ")) continue;



					// Get the current scope this instantiation code belongs to
					bool isScopeFound = false;
					for(int j = 0; j < nameSpaceList.Count; j++) {
						string currentNamespaceScope = nameSpaceList[j];
						// Check if class declaration exists in current scope
						string checkClassScope = currentNamespaceScope + "." + currentClassName;
						// Check if the class name exists in the list of declared classes
						// NOTE: The "new" keyword can be used to initiate an array as well (e.g. "𒀭PH_NEW𒀭N.C[2]"),
						// so we should remove the brackets and its content to check the actual class name existence
						if(classDeclarationList.Contains<string>(Regex.Replace(checkClassScope, @"\[(.*?)\]", ""))) {
							// It does, so we apply this scope to the declaration
							codes[i] = ReplaceFirstOccurance(codes[i], currentClassName, checkClassScope);
							isScopeFound = true;
							break;
						}
						// ...If we dont find in current namespace scope, then we traverse further up the code to find the next namespace...
					}
					// Too slow: Better code is revised above
					// Get the current scope this instantiation code belongs to (traverse back from the current code line until we find "namespace "
					/*bool isScopeFound = false;
					string currentNamespaceScope = "";
					for(int j = i; j >= 0; j--) {
						if(codes[j].IndexOf(PH_RESV_KW_NAMESPACE, 0) != -1) {
							currentNamespaceScope = codes[j].Substring(PH_RESV_KW_NAMESPACE.Length);
							// Check if class declaration exists in current scope
							string checkClassScope = currentNamespaceScope + "." + currentClassName;
							// Check if the class name exists in the list of declared classes
							// NOTE: The "new" keyword can be used to initiate an array as well (e.g. "𒀭PH_NEW𒀭N.C[2]"),
							// so we should remove the brackets and its content to check the actual class name existence
							if(classDeclarationList.Contains<string>(Regex.Replace(checkClassScope, @"\[(.*?)\]", ""))) {
								// It does, so we apply this scope to the declaration
								codes[i] = ReplaceFirstOccurance(codes[i], currentClassName, checkClassScope);
								isScopeFound = true;
								break;
							}
							// ...If we dont find in current namespace scope, then we traverse further up the code to find the next namespace...
						}
					}*/
					if(isScopeFound) continue;

					string[] usingNSClassDeclarationList = new string[classDeclarationList.Length];
					for(int j = 0; j < classDeclarationList.Length; j++) usingNSClassDeclarationList[j] = classDeclarationList[j];
					// Class declaration not found in near scope, so now we need to search into all the available namespace list in this current file
					// First of all, remove class declarations that do not belong to the "using" list
					for(int j = 0; j < usingNSClassDeclarationList.Length; j++) {
						string[] splitNS = usingNSClassDeclarationList[j].Split('.');
						string classDeclarationToCheck = "";
						for(int k = 0; k < splitNS.Length - 1; k++) {
							classDeclarationToCheck += splitNS[k] + ((k < splitNS.Length - 2) ? "." : "");
						}
						if(classDeclarationToCheck == "") classDeclarationToCheck = splitNS[0];
						// Does this class name belong to any of the names specified in the "using" definition?
						if(!usingNameList.Contains<string>(classDeclarationToCheck)) {
							usingNSClassDeclarationList[j] = "";
						}
					}
					// Now try to add scope to class declarations
					for(int j = 0; j < usingNSClassDeclarationList.Length; j++) {
						string[] splitClassName = usingNSClassDeclarationList[j].Split('.');
						if(splitClassName[splitClassName.Length - 1] == currentClassName) {
							codes[i] = ReplaceFirstOccurance(codes[i], currentClassName, usingNSClassDeclarationList[j]);
							isScopeFound = true;
							break;
						}
					}
					if(isScopeFound) continue;
				}
				return codes;
			}

			/// <summary>
			/// Add namespaces for function calls
			/// e.g.
			/// add(a, b);
			/// -->
			/// NS.CLASSNAME.add(a, b);
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveFunctionCallScopes(string[] codes) {
				// Get list of function names (should be normalized function names as prerequisite)
				List<string> funcList = new List<string>();
				for(int i = 0; i < AllFunctionKeywords.Length; i++) {
					string[] functions = GetAllKeywordDeclarations(codes, AllFunctionKeywords[i]);
					funcList.AddRange(functions.ToList<string>());
				}
				//string[] functionDeclarationList = GetAllKeywordDeclarations(codes, PH_RESV_KW_FUNCTION);
				string[] functionDeclarationList = funcList.ToArray<string>();
				// For each instantiation code found..
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(" ") != -1) continue;
					// If reserved keyword, ignore
					if(AllReservedKeywords.Contains<string>(codes[i] + " ")) continue;

					string currentFunctionName = codes[i];
					// If the function name contains a dot ".", it indicates that the namespace is already assigned to the function name so we do not have to resolve further.
					if(currentFunctionName.IndexOf(".") != -1) continue;
					if(currentFunctionName.IndexOf(PH_ID) != -1) continue;
					if(!Regex.IsMatch(currentFunctionName, @"[a-zA-Z_]")) continue;

					// Function calls without scope identifiers needs to be refering to its own class scope (or base class scope), so we traverse backwards to get this function call's class
					// It could be normal class, or static class
					string scopeClassName = "";
					for(int j = i; j >= 0; j--) {
						if(codes[j].IndexOf(PH_RESV_KW_STATIC_CLASS, 0) != -1) {
							scopeClassName = codes[j].Replace(PH_RESV_KW_STATIC_CLASS, "");
							break;
						} else if(codes[j].IndexOf(PH_RESV_KW_CLASS, 0) != -1) {
							scopeClassName = codes[j].Replace(PH_RESV_KW_CLASS, "");
							break;
						}
					}
					if(scopeClassName != "") {
						string resolvedFunctionName = scopeClassName + "." + codes[i];
						if(functionDeclarationList.Contains<string>(resolvedFunctionName)) {
							codes[i] = resolvedFunctionName;
							continue;
						}
					}


					// If we still don't find any scope class, then it means this function call is a call to the base class
					// Therefore, we will travese back the the line of codes until we find the class extend symbol ":" and search for the base class function
					// After we confirm the function exists in the base class, we can safely apply the current classes name space to this function
					// Because the base function will be copied to this class at later stage so the call to the base class function within this namespace scope will be valid
					string baseClassName = "";
					for(int j = i; j >= 0; j--) {
						if(codes[j] == ":") {
							baseClassName = codes[j + 1];
							// Check if function exists within the base class within same namespace
							bool isFunctionFound = false;
							for(int k = 0; k < functionDeclarationList.Length; k++) {
								if(functionDeclarationList[k].IndexOf(baseClassName, 0) != -1) {
									if(functionDeclarationList[k] == baseClassName + "." + codes[i]) {
										//codes[i] = functionDeclarationList[k];
										codes[i] = scopeClassName + "." + codes[i];
										isFunctionFound = true;
										break;
									}
								}
							}
							if(isFunctionFound) break;
							// Still not found? Then search deeper into parent classe(s)
							for(int k = 0; k < codes.Length; k++) {
								if(codes[k] == ":") {
									if(codes[k - 1] == PH_RESV_KW_CLASS + baseClassName) {
										// Get the next base class name
										j = k + 1;
										break;
									}
								}
							}
						}
					}
				}
				return codes;
			}


			/// <summary>
			/// Adds the specified parent scope definition to its specified child scope definition
			/// e.g. If parent scope def "namespace " (e.g. "namespace A"), and child scope def is "class ", then "class C" becomes: "class A.C"
			/// This method will iterate through the whole code
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			public string[] ResolveScope(string[] codes) {
				ShowLog(" - Resolving nested namespaces...");
				// Normalize namespace's nested namespaces
				foreach(int index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE)) {
					codes = AddKeywordScope(codes, index, PH_RESV_KW_NAMESPACE, PH_RESV_KW_NAMESPACE, "{", "}", 0);
				}

				ShowLog(" - Resolving namespace classes...");
				// Normalize namespace's classes
				foreach(int index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE)) {
					codes = AddKeywordScope(codes, index, PH_RESV_KW_NAMESPACE, PH_RESV_KW_CLASS, "{", "}", 0);
				}

				ShowLog(" - Resolving namespace static classes...");
				// Normalize namespace's static classes
				foreach(int index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE)) {
					codes = AddKeywordScope(codes, index, PH_RESV_KW_NAMESPACE, PH_RESV_KW_STATIC_CLASS, "{", "}", 0);
				}

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_1.txt", string.Join("\n", codes));

				// Join the reserved keyword list with the class names
				string[] mergedKeywordList = GenerateDefinedTypeList(codes);

				ShowLog(" - Adding namespaces for class variable definitions...");
				// Add namespaces for class variable definition (classes)
				codes = ResolveClassTypeDefinitionScopes(codes, false);

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_2.txt", string.Join("\n", codes));

				// For class extends, merge parent class to the derived class
				// Add namespaces for floating classes (e.g. casting as class type)
				//codes = ResolveClassTypeDefinitionScopes(codes, true);
				//codes = ResolveExtendedClass(codes);

				ShowLog(" - Resolving function parameters...");
				// Normalize function parameters
				List<int>[] keywordIndexListFunc = new List<int>[AllFunctionKeywords.Length];
				for(int i = 0; i < AllFunctionKeywords.Length; i++) {
					keywordIndexListFunc[i] = new List<int>();
				}
				string funcKeyword = PH_RESV_KW_FUNCTION.Trim() + "_";
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(funcKeyword) == -1) continue;
					for(int j = 0; j < AllFunctionKeywords.Length; j++) {
						if(codes[i].IndexOf(AllFunctionKeywords[j], 0, AllFunctionKeywords[j].Length) != -1) {
							keywordIndexListFunc[j].Add(i);
							break;
						}
					}
				}
				foreach(string s in mergedKeywordList) {
					for(int i = 0; i < AllFunctionKeywords.Length; i++) {
						for(int j = 0; j < keywordIndexListFunc[i].Count; j++) {
							codes = AddKeywordScope(codes, keywordIndexListFunc[i][j], AllFunctionKeywords[i], s, "(", ")");
						}
					}
				}

				// Too Slow: Updated code re-written above instead
				// Normalize function parameters
				/*foreach(string s in mergedKeywordList) {
					for(int i = 0; i < AllFunctionKeywords.Length; i++) {
						foreach(int index in GenerateKeywordIndexList(codes, AllFunctionKeywords[i])) {
							codes = AddKeywordScope(codes, index, AllFunctionKeywords[i], s, "(", ")");
						}
					}
				}*/

				ShowLog(" - Resolving static classes...");
				// Static classes should be changed to normal classes temporarly in order to correctly apply scope
				// We need to do this to resolve scope for class names that are used for object instantiation (i.e. We don't want instantiation using static classes)
				List<int> staticClassDefList = new List<int>();
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(PH_RESV_KW_STATIC_CLASS, 0) != -1) {
						codes[i] = codes[i].Replace(PH_RESV_KW_STATIC_CLASS, PH_RESV_KW_CLASS);
						// Store the code index where this static class is found, so later we can revert back only this line back to normal class
						staticClassDefList.Add(i);
					}
				}

				ShowLog(" - Resolving class properties and functions...");
				// Normalize normal class's properties and functions only
				foreach(string s in mergedKeywordList) {
					foreach(int index in GenerateKeywordIndexList(codes, PH_RESV_KW_CLASS)) {
						codes = AddKeywordScope(codes, index, PH_RESV_KW_CLASS, s, "{", "}", 0);
					}
				}

				ShowLog(" - Resolving function scope codes...");
				// Normalize everything inside a function
				foreach(string s in mergedKeywordList) {
					for(int i = 0; i < AllFunctionKeywords.Length; i++) {
						for(int j = 0; j < keywordIndexListFunc[i].Count; j++) {
							codes = AddKeywordScope(codes, keywordIndexListFunc[i][j], AllFunctionKeywords[i], s, "{", "}");
						}
					}
				}

				// Normalize everything inside a function
				// Too Slow: Updated code re-written above instead
				/*foreach(string s in mergedKeywordList) {
					for(int i = 0; i < AllFunctionKeywords.Length; i++) {
						foreach(int index in GenerateKeywordIndexList(codes, AllFunctionKeywords[i])) {
							codes = AddKeywordScope(codes, index, AllFunctionKeywords[i], s, "{", "}");
						}
					}
				}*/

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_3.txt", string.Join("\n", codes));

				ShowLog(" - Resolving variables inside class/function scope...");
				// Within the function/class scope, replace every variable with the matching variable name that is declared.
				for(int i = 0; i < AllFunctionKeywords.Length; i++) {
					codes = ApplyNormalizedVarToScope(codes, AllFunctionKeywords[i]);
				}
				codes = ApplyNormalizedVarToScope(codes, PH_RESV_KW_CLASS);

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_4.txt", string.Join("\n", codes));

				ShowLog(" - Reverting back temporary classes to static classes...");
				// Static classes that were changed to normal classes temporarly should be reverted back here
				for(int i = 0; i < staticClassDefList.Count; i++) {
					codes[staticClassDefList[i]] = codes[staticClassDefList[i]].Replace(PH_RESV_KW_CLASS, PH_RESV_KW_STATIC_CLASS);
				}

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_5.txt", string.Join("\n", codes));

				ShowLog(" - Adding namespaces for floating classes...");
				// Add namespaces for floating classes (e.g. casting as class type)
				codes = ResolveClassTypeDefinitionScopes(codes, true);

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_6.txt", string.Join("\n", codes));

				ShowLog(" - Adding namespaces and class for function calls...");
				// Add namespaces & class for function calls
				codes = ResolveFunctionCallScopes(codes);

				if(IsGenerateUnitTestFiles) File.WriteAllText("/Users/tomo/GitHub/jb-script/Test/TestPreProcessCleanUp_ResolveScope_7.txt", string.Join("\n", codes));

				ShowLog(" - Resolving \"base\" keyword scope...");
				// Resolve "base" keyword scope
				codes = ResolveBaseKeywordScope(codes);

				return codes;
			}

			/// <summary>
			/// Resolve "base" keyword scope
			/// </summary>
			/// <param name="codes"></param>
			/// <returns></returns>
			private string[] ResolveBaseKeywordScope(string[] codes) {
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(PH_RESV_KW_BASE, 0) != -1) {
						if(Regex.IsMatch(codes[i], PH_RESV_KW_BASE + "[^a-zA-Z0-9_]")) {
							// Traverse back to get the class where this base keyword belongs to
							for(int j = i; j >= 0; j--) {
								if(codes[j].IndexOf(PH_RESV_KW_CLASS, 0) != -1) {
									string className = codes[j].Replace(PH_RESV_KW_CLASS, "");
									string parentClassName = codes[j + 2];
									codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_BASE, className + "." + parentClassName);
									break;
								}
							}
						}
					}
				}

				return codes;
			}

			/// <summary>
			/// Now, we need to normalize and define scope for assignment operators and equations
			/// NOTE: "for-loops" are converted to while-loops, so there's technically no such thing as a for-loop in our programming language
			/// a = bMinusA + a;  -->  "a" and "bMinusA" should be applied with scope
			/// while(i < 20 + a[0]) {}  -->  "i" and "a" should be applied with scope
			/// if(a + 2 >= b[c[1]] + 3) {}  -->  "a" and "b" should be applied with scope
			/// return a+b;  -->  "a" and "b" should be applied with scope
			/// b[a.ARR[0]] --> "b" and "a" should be applied with scope
			/// -----------------------------------------
			/// Process: 
			/// [1] For each function scope..
			/// [2] Search for variables decalred inside the scope (e.g. int a = 1; string s = "XX";), and store it in a temporary list with its key as variable name, and value as code index
			/// [3] Within the function scope, replace every variable with the matching variable name that is declared.
			/// [4] For each class scope (and ignoring the function scope).. repeat steps [2] and [3]
			/// -----------------------------------------
			/// For each function declarations, we search for declared variables
			/// Note that declared variables have equal "=" operator,
			/// so we simply get the variable name on the left side of the operator, and the expression at the right side of the "=" operator
			/// </summary>
			/// <param name="codes"></param>
			/// <param name="reservedKeyword"></param>
			/// <returns></returns>
			private string[] ApplyNormalizedVarToScope(string[] codes, string reservedKeyword) {
				foreach(int scopeBlockBeginIndex in GenerateKeywordIndexList(codes, reservedKeyword)) {
					int scopeBlockEndIndex = FindEndScopeBlock(codes, scopeBlockBeginIndex + 1, "{", "}");

					// Resolve "this" keywords
					if(reservedKeyword == PH_RESV_KW_CLASS) {
						for(int i = scopeBlockBeginIndex; i < scopeBlockEndIndex; i++) {
							if(codes[i].IndexOf(PH_RESV_KW_THIS, 0) != -1) {
								if(Regex.IsMatch(codes[i], PH_RESV_KW_THIS + "[^a-zA-Z0-9_]") || codes[i] == PH_RESV_KW_THIS) {
									string className = ReplaceFirstOccurance(codes[scopeBlockBeginIndex], PH_RESV_KW_CLASS, "");
									codes[i] = ReplaceFirstOccurance(codes[i], PH_RESV_KW_THIS, className);
								}
							}
						}
					}

					Dictionary<string, int> d = GenerateDeclaredVarList(codes, scopeBlockBeginIndex, scopeBlockEndIndex);
					// For each declared variables, look for the variables within scope to replace
					foreach(var v in d) {
						string[] originalVarName = v.Key.Split('.');
						for(int i = scopeBlockBeginIndex; i < scopeBlockEndIndex; i++) {
							bool isOrgVarNameExists = false;
							// NOTE: a variable might have references to array index (e.g. "x[2]" , "x[2,3]"), so we check if the code contains square brackets or not
							if(codes[i].IndexOf("[") == -1) {
								if(codes[i] == originalVarName[originalVarName.Length - 1]) {
									codes[i] = codes[i].Replace(codes[i], v.Key);
									continue;
								}
								// If the full code does not contain the var name, the variable might have member calls (e.g. "obj.toString()"), so we need to remove anything after and including the dot (.)
								string[] sepDotVarName = codes[i].Split('.');
								string varCheck = sepDotVarName[0];
								if(varCheck == originalVarName[originalVarName.Length - 1]) {
									codes[i] = ReplaceFirstOccurance(codes[i], varCheck, v.Key);
									continue;
								}
							} else {
								// Separate between square brackets and get the elemnt name of the array
								string[] checkVarSplit = Regex.Split(codes[i], @"([\[\]])", RegexOptions.Singleline).Where((s, i) => s != "").ToArray();
								// Get rid of empty code and trim
								checkVarSplit = checkVarSplit.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
								// For each splitted code, check if any of the arrays contain the array name we are looking for
								for(int j = 0; j < checkVarSplit.Length; j++) {
									if(checkVarSplit[j] == originalVarName[originalVarName.Length - 1]) {
										checkVarSplit[j] = checkVarSplit[j].Replace(checkVarSplit[j], v.Key);
										isOrgVarNameExists = true;
									} else {
										// If the full code does not contain the var name, the variable might have member calls (e.g. "obj.toString()"), so we need to remove anything after and including the dot (.)
										string[] sepDotVarName = checkVarSplit[j].Split('.');
										string varCheck = sepDotVarName[0];
										if(varCheck == originalVarName[originalVarName.Length - 1]) {
											checkVarSplit[j] = ReplaceFirstOccurance(checkVarSplit[j], varCheck, v.Key);
											isOrgVarNameExists = true;
										}
									}
								}
								if(isOrgVarNameExists) {
									codes[i] = "";
									for(int j = 0; j < checkVarSplit.Length; j++) {
										codes[i] += checkVarSplit[j];
									}
								}
								/*
								// NOTE: a variable might have references to array index (e.g. "x[2]" , "x[2,3]", "x[x[2]]", "x[y[2]]"),
								// So we need to remove the array brackets for comparing with the list of variables
								// To do this, let's break down the string with separator token "["
								string[] checkVarSplit = codes[i].Split('[');
								//string[] checkVarSplit = Regex.Split(codes[i], @"[\[\]]");
								// Check if any of the arrays contain the array name we are looking for
								for(int j = 0; j < checkVarSplit.Length; j++) {
									if(checkVarSplit[j] == originalVarName[originalVarName.Length - 1]) {
										checkVarSplit[j] = checkVarSplit[j].Replace(checkVarSplit[j], v.Key);
										isOrgVarNameExists = true;
									} else {
										// If the full code does not contain the var name, the variable might have member calls (e.g. "obj.toString()"), so we need to remove anything after and including the dot (.)
										string[] sepDotVarName = checkVarSplit[j].Split('.');
										string varCheck = sepDotVarName[0];
										if(varCheck== originalVarName[originalVarName.Length - 1]) {
											checkVarSplit[j] = ReplaceFirstOccurance(checkVarSplit[j], varCheck, v.Key);
											isOrgVarNameExists = true;
										}
									}
								}
								if(isOrgVarNameExists) {
									codes[i] = "";
									for(int j = 0; j < checkVarSplit.Length; j++) {
										codes[i] += checkVarSplit[j] + "[";
									}
									codes[i] = codes[i].Substring(0, codes[i].Length - 1);
								}*/
							}
							/*
							// NOTE: a variable might have member calls (e.g. "obj.toString()"), so we need to remove anything after and including the dot (.)
							if(codes[i].IndexOf(".") == -1) continue;
							// Similarly, we separate by dot and check if var name exists
							string[] sepDotVarName = codes[i].Split('.');
							isOrgVarNameExists = false;
							for(int j = 0; j < sepDotVarName.Length; j++) {
								if(sepDotVarName[j] == originalVarName[originalVarName.Length - 1]) {
									sepDotVarName[j] = sepDotVarName[j].Replace(sepDotVarName[j], v.Key);
									isOrgVarNameExists = true;
								}
							}
							if(isOrgVarNameExists) {
								codes[i] = "";
								for(int j = 0; j < sepDotVarName.Length; j++) {
									codes[i] += sepDotVarName[j] + ".";
								}
								codes[i] = codes[i].Substring(0, codes[i].Length - 1);
							}*/
							/*
							string varCheck = sep[0];
							if(varCheck == originalVarName[originalVarName.Length - 1]) {
								codes[i] = ReplaceFirstOccurance(codes[i], varCheck, v.Key);
								continue;
							}*/
						}
					}
				}
				return codes;
			}

			private Dictionary<string, int> GenerateDeclaredVarList(string[] codes, int scopeBlockBeginIndex, int scopeBlockEndIndex) {
				var varList = new Dictionary<string, int>();
				for(int i = scopeBlockBeginIndex; i < scopeBlockEndIndex; i++) {
					string[] splitCode = codes[i].Split(' ');
					if(splitCode.Length < 2) continue;
					string varName = splitCode[1];
					if(NonMethodKeywords.Contains<string>(splitCode[0])) continue;
					if(!NonVariableDeclareKeywords.Contains<string>(splitCode[0] + " ")) {
						// Found a variable declaration
						varList.Add(varName, i);
					}
					/*
					// Found an assignment operator, or declarations inside round brackets (which is a function parameter)
					if(codes[i] == "=" || codes[i] == "," || codes[i] == ")") {
						// Get the variable declaration (e.g. "string Ankisua.Game.add.asd")
						string[] varDeclare = codes[i - 1].Split(' ');
						// If there's no type declaration, it means this is not a variable declaration
						if(varDeclare.Length != 2) continue;
						varList.Add(varDeclare[1], i);
					}*/
				}
				return varList;
			}

			/// <summary>
			/// Stores the indexes of the keyword in the code string array
			/// </summary>
			/// <param name="codes">The string array of tokenized code</param>
			/// <param name="keyword">Scope keyword definition (typically with a trailing space)</param>
			/// <returns></returns>
			private List<int> GenerateKeywordIndexList(string[] codes, string keyword) {
				List<int> keywordIndexList = new List<int>();
				for(int i = 0; i < codes.Length; i++) {
					if(codes[i].IndexOf(keyword) == -1) continue;
					if(codes[i].IndexOf(keyword, 0, keyword.Length) != -1) {
						keywordIndexList.Add(i);
					}
				}
				return keywordIndexList;
			}

			/// <summary>
			/// Adds the specified parent scope definition to its specified child scope definition
			/// e.g. If parent scope def "namespace " (e.g. "namespace A"), and child scope def is "class ", then "class C" becomes: "class A.C"
			/// </summary>
			/// <param name="codes">The string array of tokenized code</param>
			/// <param name="startCodeArrayIndex">The array index in the code string array to start searching for the keyword</param>
			/// <param name="parentScopeKeyword">Parent scope keyword definition with a trailing space</param>
			/// <param name="childScopeKeyword">Child scope keyword definition with a trailing space</param>
			/// <param name="openingScopeBlock">The keyword which indicates the opening of a block (e.g. "{", "(", "[")</param>
			/// <param name="endingScopeBlock">The keyword which indicates the opening of a block (e.g. "}", ")", "]")</param>
			/// <param name="limitToScopeHierarchyLevel">typically, namespace block is level 0, and classes are level 1 block, functions are level 2</param>
			/// <returns>The string array of tokenized code with removed parent definition code, and child scope definition with added parent scope defition keyword</returns>
			public string[] AddKeywordScope(string[] codes, int startCodeArrayIndex, string parentScopeKeyword, string childScopeKeyword, string openingScopeBlock, string endingScopeBlock, int limitToScopeHierarchyLevel = -1) {
				// Search for keyword namespace
				int scopeBlockBeginIndex = -1;

				string parentScopeDefName = ReplaceFirstOccurance(codes[startCodeArrayIndex], parentScopeKeyword, "");
				codes[startCodeArrayIndex] = parentScopeKeyword + parentScopeDefName;

				// Get the first index where the scope parenthesis begins
				for(int i = startCodeArrayIndex + 1; i < codes.Length; i++) {
					if(codes[i] == openingScopeBlock) {
						scopeBlockBeginIndex = i;
						break;
					}
				}

				int scopeBlockEndIndex = FindEndScopeBlock(codes, scopeBlockBeginIndex, openingScopeBlock, endingScopeBlock);
				string replaceWith = childScopeKeyword + parentScopeDefName + ".";
				codes = ReplaceScopeKeywordsWith(codes, scopeBlockBeginIndex, scopeBlockEndIndex, openingScopeBlock, endingScopeBlock, childScopeKeyword, replaceWith, limitToScopeHierarchyLevel);

				return codes;
			}

			public string[] ReplaceScopeKeywordsWith(string[] codes, int scopeBlockBeginIndex, int scopeBlockEndIndex, string openingScopeBlock, string endingScopeBlock, string childScopeKeyword, string replaceWith, int limitToScopeHierarchyLevel, bool isOnlyReplaceMethods = false) {
				int scopeHierarchyLevel = -1;
				// Inside the scope block of the namespace, add the namespace def string in front of each class
				// So if namespace = "namespace A", then "class C" becomes: "class A.C"
				// If there are nested namespaces, for example "namespace A { namespace B {" then "class C" inside namespace B becomes "class A.B.C"
				for(int i = scopeBlockBeginIndex; i < scopeBlockEndIndex; i++) {
					if(codes[i] == openingScopeBlock) scopeHierarchyLevel++;
					if(codes[i] == endingScopeBlock) scopeHierarchyLevel--;
					if(limitToScopeHierarchyLevel != -1) {
						if(scopeHierarchyLevel != limitToScopeHierarchyLevel) {
							continue;
						}
					}
					if(codes[i].IndexOf(childScopeKeyword) == -1) continue;
					if(isOnlyReplaceMethods) {
						// Methods have a round bracket after the declaration
						// i.e. int MethodName() --> [0] = "int MethodName", [1] = "(", [2] = ")"
						// So we check if the target code has a opening round bracket "(" after in its code array and determine if it is a method or not
						if(codes[i + 1] != "(") continue;
					}
					if(codes[i].IndexOf(childScopeKeyword, 0, childScopeKeyword.Length) != -1) {

						/*
						// If there's already a namespace applied to a class, it means the class will have nested nameclasses
						// So, we will need to shift the new namespace name after the last dot "."
						// e.g. "namespace B" to be added to --> class A.Test --> class A.B.Test
						if(codes[i].IndexOf(".") != -1) {
							// Get the current class name only with the proceeding dot
							string currentNameSpace = codes[i].Replace(childScopeKeyword, "");
							string newNameSpace = replaceWith.Replace(childScopeKeyword, "");
							string[] splitNestedNamespace = codes[i].Split('.');
							replaceWith = "";
							for(int j = 0; j < splitNestedNamespace.Length; j++) {
								if(j == splitNestedNamespace.Length - 1) {
									replaceWith += "." + newNameSpace + splitNestedNamespace[j];
								} else {
									replaceWith += splitNestedNamespace[j];
								}
							}
						}
						*/
						codes[i] = ReplaceFirstOccurance(codes[i], childScopeKeyword, replaceWith);
					}
				}
				return codes;
			}

			#endregion
		}
	}
}

