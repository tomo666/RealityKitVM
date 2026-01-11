//
//  VMBase.swift
//  JBScript
//
//  Created by Tomohiro Kadono on 2026/01/02.
//

import Foundation

/// JBScript Virtual Machine (Base Layer)
///
/// - Notes:
///   - This is a fairly direct port of the original C# base VM.
///   - Designed to be subclassed by platform layers (Unity/Godot/etc.).
///   - Uses big-endian byte layout inside `virtualMemory`, matching the C# implementation
///     (C# wrote bytes reversed from `BitConverter.GetBytes` and read them back accordingly).
open class VMBase {

    // MARK: - Commands (8-bit)
    internal static let NOP: UInt8 = 0x0
    internal static let VMC: UInt8 = 0x1
    internal static let VAR: UInt8 = 0x2
    internal static let MOV: UInt8 = 0x3
    internal static let JMP: UInt8 = 0x4
    internal static let JMS: UInt8 = 0x5
    internal static let RET: UInt8 = 0x6
    internal static let ADD: UInt8 = 0x7
    internal static let SUB: UInt8 = 0x8
    internal static let MLT: UInt8 = 0x9
    internal static let DIV: UInt8 = 0xA
    internal static let MOD: UInt8 = 0xB
    internal static let SHL: UInt8 = 0xC
    internal static let SHR: UInt8 = 0xD
    internal static let CAN: UInt8 = 0xE
    internal static let COR: UInt8 = 0xF
    internal static let SKE: UInt8 = 0x10
    internal static let AND: UInt8 = 0x11
    internal static let LOR: UInt8 = 0x12
    internal static let XOR: UInt8 = 0x13
    internal static let PSH: UInt8 = 0x14
    internal static let POP: UInt8 = 0x15
    internal static let LEQ: UInt8 = 0x16
    internal static let LNE: UInt8 = 0x17
    internal static let LSE: UInt8 = 0x18
    internal static let LGE: UInt8 = 0x19
    internal static let LGS: UInt8 = 0x1A
    internal static let LGG: UInt8 = 0x1B
    internal static let UMN: UInt8 = 0x1C
    internal static let UPL: UInt8 = 0x1D
    internal static let UNT: UInt8 = 0x1E
    internal static let BUC: UInt8 = 0x1F

    // MARK: - Types (8-bit)
    internal static let TYPE_NULL: UInt8  = 0x0
    internal static let TYPE_PTR: UInt8   = 0x1
    internal static let TYPE_IMM: UInt8   = 0x2
    internal static let TYPE_UVAR8: UInt8  = 0x3
    internal static let TYPE_SVAR8: UInt8  = 0x4
    internal static let TYPE_UVAR16: UInt8 = 0x5
    internal static let TYPE_SVAR16: UInt8 = 0x6
    internal static let TYPE_UVAR32: UInt8 = 0x7
    internal static let TYPE_SVAR32: UInt8 = 0x8
    internal static let TYPE_UVAR64: UInt8 = 0x9
    internal static let TYPE_SVAR64: UInt8 = 0xA

    // MARK: - Pre-defined VMC call IDs
    internal static let VMC_PRINT: Int64 = 0
    internal static let VMC_MALLOC: Int64 = 1
    internal static let VMC_REG_MALLOC_GROUP_SIZE: Int64 = 2
    internal static let VMC_RUN_GC: Int64 = 3
    internal static let VMC_SET_FLOAT_MULTBY_FACTOR: Int64 = 4
    internal static let VMC_FREE: Int64 = 5
    internal static let VMC_SIZEOF_ALLOCMEM: Int64 = 6
    internal static let VMC_FILE_OPEN: Int64 = 7
    internal static let VMC_FILE_CLEAR_BUFFER: Int64 = 8
    internal static let VMC_FILE_APPEND_VM_BYTES: Int64 = 9
    internal static let VMC_FILE_SAVE: Int64 = 10
    internal static let VMC_FILE_READ_VM_BYTES: Int64 = 11
    internal static let VMC_FILE_LOAD: Int64 = 12

    // MARK: - Constants
    internal static let MAX_OPCODE_BYTE_SIZE: Int = 2 // Command(1) + OpType(1)

    // MARK: - Config
    private(set) public var maxBitSize: Int
    private(set) public var maxOpcodeValueSize: Int

    /// Decimal point precision multiplying factor (set by compiler, configured via VMC call)
    public internal(set) var decPtMltFactor: Double = 0

    // MARK: - Program storage
    public internal(set) var binCode: [UInt8] = []

    // MARK: - VM State
    private var pc: Int = 0
    private var useVar: Int64 = 0
    private var useVarOpType: UInt8 = 0

    // Virtual stacks
    private var subReturnIndexStack: [Int] = []
    private var commonValueStack: [Int64] = []

    // Virtual memory
    public internal(set) var virtualMemory: [UInt8] = []

    /// Variable ID -> virtual memory address (index)
    public internal(set) var varAddrTable: [Int] = []

    /// Variable ID -> variable size (bytes)
    public internal(set) var varMemSizeTable: [Int] = []

    /// Allocated address -> allocated byte size
    public internal(set) var mallocAddressSizeTable: [Int: Int] = [:]

    /// Holds variable IDs (indices) that are static pointer variables
    public internal(set) var staticInstanceObjectList: [Int] = []

    // Temporary byte storage (8 bytes)
    private var tmpBytes: [UInt8] = Array(repeating: 0, count: 8)

    // File streaming
    private var fileStreamFileName: String = ""
    private var fileStreamBuffer: [UInt8] = []

    // MARK: - Init
    public init(maxBitSize: Int = 32) {
        self.maxBitSize = maxBitSize
        self.maxOpcodeValueSize = maxBitSize / 8
    }

    // MARK: - Load
    open func loadBinFile(_ binFile: String) throws {
        let data = try Data(contentsOf: URL(fileURLWithPath: binFile))
        binCode = Array(data)
        initializeVM()
    }

    // MARK: - Compile
    open func CompileScript(_ scriptFile: String, _ isWriteBinFile: Bool = true) -> [UInt8]? {
        // Generate bin file name from script file name (test.cs --> test.bin)
        let binFileName = URL(fileURLWithPath: scriptFile).deletingPathExtension().appendingPathExtension("bytes").path.replacingOccurrences(of: "Assets/DScripts~", with: "Assets/Bin")
        print("Generating \(binFileName)")
        // --- Create compile options ---
        let options = CompileOptions(FileName: scriptFile, BinFileName: binFileName, IsOutputIntermCode: false, MaxBitSize: 32, DecimalPointPrecision: 8)
        // --- Run compiler ---
        return JBScript.Compile(options, isWriteBinFile)
    }
    
    // MARK: - Initialize
    open func initializeVM() {
        pc = 0
        useVar = 0
        useVarOpType = 0
        decPtMltFactor = 0

        subReturnIndexStack.removeAll(keepingCapacity: true)
        commonValueStack.removeAll(keepingCapacity: true)
        virtualMemory.removeAll(keepingCapacity: true)
        varAddrTable.removeAll(keepingCapacity: true)
        varMemSizeTable.removeAll(keepingCapacity: true)
        mallocAddressSizeTable.removeAll(keepingCapacity: true)
        staticInstanceObjectList.removeAll(keepingCapacity: true)
        fileStreamBuffer.removeAll(keepingCapacity: true)

        registerVariables()
    }

    // MARK: - Register Variables
    private func registerVariables() {
        // The first few non-VAR commands are usually JMS to main entry, so ignore until we find the first VAR.
        var isVarCmdFound = false

        while true {
            guard pc + 1 < binCode.count else { break }

            let cmd = binCode[pc]
            let type = binCode[pc + 1]

            if cmd == VMBase.VAR && !isVarCmdFound {
                isVarCmdFound = true
            }

            // If we reached the first non-VAR after we started seeing VAR, stop.
            if cmd != VMBase.VAR && isVarCmdFound {
                break
            }

            if isVarCmdFound {
                let byteSize = Int(getByteSizeFromOperationType(type))
                registerVarToVirtualMemory(startBinCodeIndex: pc + VMBase.MAX_OPCODE_BYTE_SIZE, byteSize: byteSize)

                if type == VMBase.TYPE_PTR {
                    staticInstanceObjectList.append(varAddrTable.count - 1)
                }
            }

            pc += VMBase.MAX_OPCODE_BYTE_SIZE + maxOpcodeValueSize
            if pc >= binCode.count { break }
        }

        // Run remaining program after VAR declarations (this should run init method, etc.)
        run(runStep: -1, programCounter: pc)

        // Reset program counter to the beginning
        pc = 0
    }

    // MARK: - Byte sizing
    internal func getByteSizeFromOperationType(_ opType: UInt8) -> UInt8 {
        switch opType {
        case VMBase.TYPE_UVAR8, VMBase.TYPE_SVAR8: return 1
        case VMBase.TYPE_UVAR16, VMBase.TYPE_SVAR16: return 2
        case VMBase.TYPE_UVAR32, VMBase.TYPE_SVAR32: return 4
        case VMBase.TYPE_UVAR64, VMBase.TYPE_SVAR64: return 8
        case VMBase.TYPE_NULL: return 0
        default:
            return UInt8(maxBitSize / 8)
        }
    }

    // MARK: - BinCode value decode (big-endian)
    internal func getBinCodeValue(startAddressIndex: Int, byteSize: Int, byteCodeList: [UInt8]) -> Int64 {
        var value: Int64 = 0
        let addr = startAddressIndex
        guard byteSize > 0 else { return 0 }
        for i in 0..<byteSize {
            value <<= 8
            value |= Int64(byteCodeList[addr + i])
        }
        return value
    }

    // MARK: - Virtual memory variable registration
    private func registerVarToVirtualMemory(startBinCodeIndex: Int, byteSize: Int) {
        // Store variable address
        varAddrTable.append(virtualMemory.count)
        // Store variable size
        varMemSizeTable.append(byteSize)
        // Initialize with zeros
        if byteSize > 0 {
            virtualMemory.append(contentsOf: Array(repeating: 0, count: byteSize))
        }
        _ = startBinCodeIndex // kept for parity; the original VM ignored initial value and zero-initialized.
    }

    // MARK: - Virtual memory set/get (big-endian layout)
    internal func setValueToVirtualMemory(vMemAddress: Int, byteSize: UInt8, value: Int64) {
        let size = Int(byteSize)
        guard size > 0 else { return }
        let bytes = toLittleEndianBytes(value)
        // Write as big-endian into virtualMemory (same as C# reverse write)
        for i in 0..<size {
            virtualMemory[vMemAddress + i] = bytes[size - i - 1]
        }
    }

    /// Reads a signed integer value from virtual memory.
    /// - Note: This matches the original C# behavior. Values are stored as big-endian bytes.
    open func getValueFromVirtualMemory(vMemAddress: Int, byteSize: UInt8) -> Int64 {
        let size = Int(byteSize)
        guard size > 0 else { return 0 }

        // Reset tmpBytes
        for i in 0..<tmpBytes.count { tmpBytes[i] = 0 }

        // Convert big-endian memory bytes into little-endian buffer
        for i in 0..<size {
            tmpBytes[size - 1 - i] = virtualMemory[vMemAddress + i]
        }

        switch size {
        case 1:
            return Int64(tmpBytes[0])
        case 2:
            return Int64(Int16(bitPattern: UInt16(tmpBytes[0]) | (UInt16(tmpBytes[1]) << 8)))
        case 4:
            let u = UInt32(tmpBytes[0]) | (UInt32(tmpBytes[1]) << 8) | (UInt32(tmpBytes[2]) << 16) | (UInt32(tmpBytes[3]) << 24)
            return Int64(Int32(bitPattern: u))
        case 8:
            let u = UInt64(tmpBytes[0])
                | (UInt64(tmpBytes[1]) << 8)
                | (UInt64(tmpBytes[2]) << 16)
                | (UInt64(tmpBytes[3]) << 24)
                | (UInt64(tmpBytes[4]) << 32)
                | (UInt64(tmpBytes[5]) << 40)
                | (UInt64(tmpBytes[6]) << 48)
                | (UInt64(tmpBytes[7]) << 56)
            return Int64(bitPattern: u)
        default:
            // Fallback: read up to 8 bytes
            var u: UInt64 = 0
            let n = min(size, 8)
            for i in 0..<n {
                u |= UInt64(tmpBytes[i]) << (UInt64(i) * 8)
            }
            return Int64(bitPattern: u)
        }
    }

    // MARK: - Value indirection helpers
    internal func setActualValueByType(binCodeValue: Int64, opType: UInt8, value: Int64) {
        let byteSize = getByteSizeFromOperationType(opType)

        if opType == VMBase.TYPE_PTR {
            // Pointer variable contains an address into virtual memory
            let variableAddress = varAddrTable[Int(binCodeValue)]
            let actualAddress = Int(getValueFromVirtualMemory(vMemAddress: variableAddress, byteSize: byteSize))

            // Find which variable owns `actualAddress` to get its true byte size
            var actualVar: Int = 0
            for i in stride(from: varAddrTable.count - 1, through: 0, by: -1) {
                if varAddrTable[i] == actualAddress {
                    actualVar = i
                    break
                }
            }

            setValueToVirtualMemory(vMemAddress: actualAddress, byteSize: UInt8(varMemSizeTable[actualVar]), value: value)
        } else {
            setValueToVirtualMemory(vMemAddress: varAddrTable[Int(binCodeValue)], byteSize: byteSize, value: value)
        }
    }

    internal func getActualValueByType(binCodeValue: Int64, opType: UInt8) -> Int64 {
        let byteSize = getByteSizeFromOperationType(opType)

        if opType == VMBase.TYPE_IMM {
            return binCodeValue
        }

        if opType == VMBase.TYPE_PTR {
            let variableAddress = varAddrTable[Int(binCodeValue)]
            let actualAddress = Int(getValueFromVirtualMemory(vMemAddress: variableAddress, byteSize: byteSize))

            var actualVar: Int = 0
            for i in stride(from: varAddrTable.count - 1, through: 0, by: -1) {
                if varAddrTable[i] == actualAddress {
                    actualVar = i
                    break
                }
            }

            return getValueFromVirtualMemory(vMemAddress: actualAddress, byteSize: UInt8(varMemSizeTable[actualVar]))
        }

        return getValueFromVirtualMemory(vMemAddress: varAddrTable[Int(binCodeValue)], byteSize: byteSize)
    }

    // MARK: - Run
    open func run(runStep: Int = -1, programCounter: Int = 0) {
        pc = programCounter
        var stepsLeft = runStep

        while true {
            var isSkipProgramCount = false

            guard pc + 1 < binCode.count else { break }

            let cmd = binCode[pc]
            let opType = binCode[pc + 1]

            var val: Int64 = 0
            if opType != VMBase.TYPE_NULL {
                val = getBinCodeValue(startAddressIndex: pc + VMBase.MAX_OPCODE_BYTE_SIZE, byteSize: maxOpcodeValueSize, byteCodeList: binCode)
            }

            switch cmd {
            case VMBase.VAR:
                useVar = val
                useVarOpType = opType

            case VMBase.VMC:
                virtualMachineCall(callID: getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.MOV:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.JMP:
                pc = Int(getActualValueByType(binCodeValue: val, opType: opType))
                isSkipProgramCount = true

            case VMBase.JMS:
                subReturnIndexStack.append(pc + VMBase.MAX_OPCODE_BYTE_SIZE + maxOpcodeValueSize)
                pc = Int(getActualValueByType(binCodeValue: val, opType: opType))
                isSkipProgramCount = true

            case VMBase.RET:
                if let ret = subReturnIndexStack.last {
                    pc = ret
                    subReturnIndexStack.removeLast()
                    isSkipProgramCount = true
                } else {
                    // Safety: empty return stack
                    return
                }

            case VMBase.ADD:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     + getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.SUB:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     - getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.MLT:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     * getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.DIV:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     / getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.MOD:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     % getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.SHL:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     << Int(getActualValueByType(binCodeValue: val, opType: opType)))

            case VMBase.SHR:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                     >> Int(getActualValueByType(binCodeValue: val, opType: opType)))

            case VMBase.CAN:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                let b = getActualValueByType(binCodeValue: val, opType: opType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (a && b) ? 1 : 0)

            case VMBase.COR:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                let b = getActualValueByType(binCodeValue: val, opType: opType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (a || b) ? 1 : 0)

            case VMBase.SKE:
                // NOTE: pc + 2 + valueSize => [start of next opcode], and +1 => opType of that next opcode
                if getActualValueByType(binCodeValue: useVar, opType: useVarOpType) == getActualValueByType(binCodeValue: val, opType: opType) {
                    let nextOpTypeIndex = pc + VMBase.MAX_OPCODE_BYTE_SIZE + maxOpcodeValueSize + 1
                    if nextOpTypeIndex < binCode.count {
                        let nextOpType = binCode[nextOpTypeIndex]
                        pc += Int(getByteSizeFromOperationType(nextOpType)) + VMBase.MAX_OPCODE_BYTE_SIZE
                    }
                }

            case VMBase.AND:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                let b = getActualValueByType(binCodeValue: val, opType: opType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (a && b) ? 1 : 0)

            case VMBase.LOR:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                let b = getActualValueByType(binCodeValue: val, opType: opType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (a || b) ? 1 : 0)

            case VMBase.XOR:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                let b = getActualValueByType(binCodeValue: val, opType: opType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (a != b) ? 1 : 0)

            case VMBase.PSH:
                commonValueStack.append(getActualValueByType(binCodeValue: val, opType: opType))

            case VMBase.POP:
                if let popped = commonValueStack.last {
                    setActualValueByType(binCodeValue: val, opType: opType, value: popped)
                    commonValueStack.removeLast()
                } else {
                    // Safety: underflow
                    setActualValueByType(binCodeValue: val, opType: opType, value: 0)
                }

            case VMBase.LEQ:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             == getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.LNE:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             != getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.LSE:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             <= getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.LGE:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             >= getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.LGS:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             < getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.LGG:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: (getActualValueByType(binCodeValue: useVar, opType: useVarOpType)
                                             > getActualValueByType(binCodeValue: val, opType: opType)) ? 1 : 0)

            case VMBase.UMN:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: -getActualValueByType(binCodeValue: useVar, opType: useVarOpType))
                pc += VMBase.MAX_OPCODE_BYTE_SIZE
                isSkipProgramCount = true

            case VMBase.UPL:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: +getActualValueByType(binCodeValue: useVar, opType: useVarOpType))
                pc += VMBase.MAX_OPCODE_BYTE_SIZE
                isSkipProgramCount = true

            case VMBase.UNT:
                let a = getActualValueByType(binCodeValue: useVar, opType: useVarOpType) != 0
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: (!a) ? 1 : 0)
                pc += VMBase.MAX_OPCODE_BYTE_SIZE
                isSkipProgramCount = true

            case VMBase.BUC:
                setActualValueByType(binCodeValue: useVar, opType: useVarOpType,
                                     value: ~getActualValueByType(binCodeValue: useVar, opType: useVarOpType))
                pc += VMBase.MAX_OPCODE_BYTE_SIZE
                isSkipProgramCount = true

            default:
                // NOP or unknown
                break
            }

            // Next instruction
            if !isSkipProgramCount {
                pc += VMBase.MAX_OPCODE_BYTE_SIZE + maxOpcodeValueSize
            }

            if pc >= binCode.count { break }
            if stepsLeft == -1 { continue }
            stepsLeft -= 1
            if stepsLeft == 0 { break }
        }
    }

    // MARK: - Stack helpers
    open func popValueFromStack() -> Int64 {
        let v = commonValueStack.last ?? 0
        if !commonValueStack.isEmpty { commonValueStack.removeLast() }
        return v
    }

    open func pushValueToStack(_ value: Int64) {
        commonValueStack.append(value)
    }

    // MARK: - Memory release
    open func releaseMemory(addressToRelease: Int, numBytesToRelease: Int) {
        // Check if we have any variables pointing to this address
        if let varID = varAddrTable.firstIndex(of: addressToRelease) {

            // Update reference addresses of static pointer vars beyond the deleted block
            for varToCheck in staticInstanceObjectList {
                let varVirtualMemoryAddress = varAddrTable[varToCheck]
                let varSize = UInt8(varMemSizeTable[varToCheck])
                let pointingAtAddress = Int(getValueFromVirtualMemory(vMemAddress: varVirtualMemoryAddress, byteSize: varSize))
                if pointingAtAddress >= addressToRelease + numBytesToRelease {
                    setValueToVirtualMemory(vMemAddress: varVirtualMemoryAddress, byteSize: varSize, value: Int64(pointingAtAddress - numBytesToRelease))
                }
            }

            // Remove the bytes from virtual memory
            if numBytesToRelease > 0, addressToRelease >= 0, addressToRelease + numBytesToRelease <= virtualMemory.count {
                virtualMemory.removeSubrange(addressToRelease..<(addressToRelease + numBytesToRelease))
            }

            // Remove variable references for the number of bytes allocated
            var bytesRemoved = numBytesToRelease
            var idx = varID
            while bytesRemoved != 0, idx < varMemSizeTable.count {
                bytesRemoved -= varMemSizeTable[idx]
                varAddrTable.remove(at: idx)
                varMemSizeTable.remove(at: idx)
                // Do not increment idx because we removed the current index
            }

            // Remove the allocation entry if it exists (keyed by address)
            mallocAddressSizeTable.removeValue(forKey: addressToRelease)

            // Update malloc table keys beyond the deleted region
            if !mallocAddressSizeTable.isEmpty {
                var newTable: [Int: Int] = [:]
                newTable.reserveCapacity(mallocAddressSizeTable.count)
                for (k, v) in mallocAddressSizeTable {
                    if k > addressToRelease {
                        newTable[k - numBytesToRelease] = v
                    } else if k < addressToRelease {
                        newTable[k] = v
                    }
                }
                mallocAddressSizeTable = newTable
            }

        } else {
            // No variables are pointing to this address, so just remove bytes
            if numBytesToRelease > 0, addressToRelease >= 0, addressToRelease + numBytesToRelease <= virtualMemory.count {
                virtualMemory.removeSubrange(addressToRelease..<(addressToRelease + numBytesToRelease))
            }
        }
    }

    // MARK: - String generation
    /// Generate a Swift String from VM memory that stores UTF-16 code units (2 bytes per char) terminated by 0.
    open func generateStringFromAddress(_ stringStartAddress: Int) -> String {
        var scalars: [UInt16] = []
        var i = stringStartAddress
        while i + 1 < virtualMemory.count {
            let v = UInt16(truncatingIfNeeded: getValueFromVirtualMemory(vMemAddress: i, byteSize: 2))
            if v == 0 { break }
            scalars.append(v)
            i += 2
        }
        return String(decoding: scalars, as: UTF16.self)
    }

    // MARK: - Virtual machine calls
    /// Override in platform layers to provide additional functions.
    /// NOTE: IDs 0...255 are reserved.
    open func virtualMachineCall(callID: Int64) {
        switch callID {
        case VMBase.VMC_PRINT:
            let str = generateStringFromAddress(Int(popValueFromStack()))
            print(str)

        case VMBase.VMC_MALLOC:
            let bytesToAllocate = Int(popValueFromStack())
            let newStart = virtualMemory.count
            if bytesToAllocate > 0 {
                virtualMemory.append(contentsOf: Array(repeating: 0, count: bytesToAllocate))
            }
            varAddrTable.append(newStart)
            varMemSizeTable.append(bytesToAllocate)
            // Set allocated start address into current selected var
            setActualValueByType(binCodeValue: useVar, opType: useVarOpType, value: Int64(virtualMemory.count - bytesToAllocate))

        case VMBase.VMC_REG_MALLOC_GROUP_SIZE:
            let totalMemBlockSize = Int(popValueFromStack())
            let startAddress = Int(popValueFromStack())
            mallocAddressSizeTable[startAddress] = totalMemBlockSize

        case VMBase.VMC_RUN_GC:
            // GC intentionally not implemented in this base port (same as C# which had it commented out).
            break

        case VMBase.VMC_SET_FLOAT_MULTBY_FACTOR:
            decPtMltFactor = Double(popValueFromStack())

        case VMBase.VMC_FREE:
            let numBytesToRelease = Int(popValueFromStack())
            let addressToRelease = Int(popValueFromStack())
            releaseMemory(addressToRelease: addressToRelease, numBytesToRelease: numBytesToRelease)

        case VMBase.VMC_SIZEOF_ALLOCMEM:
            let instanceAddress = Int(popValueFromStack())
            if let size = mallocAddressSizeTable[instanceAddress] {
                pushValueToStack(Int64(size))
            }

        case VMBase.VMC_FILE_OPEN:
            fileStreamFileName = generateStringFromAddress(Int(popValueFromStack()))
            pushValueToStack(FileManager.default.fileExists(atPath: fileStreamFileName) ? 1 : 0)

        case VMBase.VMC_FILE_CLEAR_BUFFER:
            fileStreamBuffer.removeAll(keepingCapacity: true)

        case VMBase.VMC_FILE_APPEND_VM_BYTES:
            let numBytesToWrite = Int(popValueFromStack())
            let startVMAddress = Int(popValueFromStack())
            guard numBytesToWrite > 0 else { break }
            let end = min(startVMAddress + numBytesToWrite, virtualMemory.count)
            if startVMAddress >= 0, startVMAddress < end {
                fileStreamBuffer.append(contentsOf: virtualMemory[startVMAddress..<end])
            }

        case VMBase.VMC_FILE_SAVE:
            do {
                let url = URL(fileURLWithPath: fileStreamFileName)
                try Data(fileStreamBuffer).write(to: url)
                pushValueToStack(1)
            } catch {
                pushValueToStack(0)
            }

        case VMBase.VMC_FILE_LOAD:
            do {
                let url = URL(fileURLWithPath: fileStreamFileName)
                let data = try Data(contentsOf: url)
                fileStreamBuffer.append(contentsOf: Array(data))
                pushValueToStack(1)
            } catch {
                pushValueToStack(0)
            }

        case VMBase.VMC_FILE_READ_VM_BYTES:
            let numBytesToRead = Int(popValueFromStack())
            let startStreamAddress = Int(popValueFromStack())
            var writeStartVMAddress = Int(popValueFromStack())
            guard numBytesToRead > 0 else { break }

            let endStream = min(startStreamAddress + numBytesToRead, fileStreamBuffer.count)
            if startStreamAddress >= 0, startStreamAddress < endStream {
                for i in startStreamAddress..<endStream {
                    if writeStartVMAddress >= 0 && writeStartVMAddress < virtualMemory.count {
                        virtualMemory[writeStartVMAddress] = fileStreamBuffer[i]
                        writeStartVMAddress += 1
                    } else {
                        break
                    }
                }
            }

        default:
            // Unknown call ID (platform layer can override and handle custom IDs)
            break
        }
    }

    // MARK: - Utilities
    private func toLittleEndianBytes(_ value: Int64) -> [UInt8] {
        let u = UInt64(bitPattern: value)
        return [
            UInt8(truncatingIfNeeded: u & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 8) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 16) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 24) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 32) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 40) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 48) & 0xFF),
            UInt8(truncatingIfNeeded: (u >> 56) & 0xFF)
        ]
    }
}
