//
//  Compiler.swift
//  JBScript
//
//  Created by Tomohiro Kadono on 2025/12/25.
//

import Foundation
struct CompileOptions {
    var FileName: String
    var BinFileName: String
    var IsOutputIntermCode: Bool
    var MaxBitSize: Int
    var DecimalPointPrecision: Int
}

enum JBScript {
    // if unit testing, enable this flag
    public static var IsGenerateUnitTestFiles = false
    // For testing purpose
    public static var TestLoadExternalIncludesString: String = ""
    public static var TestSplitCodesToArray: [String]? = nil
    public static var TestAddMissingScopeBrackets: [String]? = nil
    public static var TestResolveInstantiationCode: [String]? = nil
    public static var TestConvertStringKeywordToCharArray: [String]? = nil
    public static var TestReplacePreDefinedKeywordsWithActualValue: [String]? = nil
    public static var TestResolveFloatDoubleSuffix: [String]? = nil
    public static var TestConvertArraysToCLangPointerNotations: [String]? = nil
    public static var TestConvertMethodToFunctions: [String]? = nil
    public static var TestAddReturnToFunctions: [String]? = nil
    public static var TestResolveReturnStatements: [String]? = nil
    public static var TestResolveScope: [String]? = nil
    public static var TestResolveScope_Phase1: [String]? = nil
    public static var TestResolveScope_Phase2: [String]? = nil
    public static var TestResolveScope_Phase3: [String]? = nil
    public static var TestResolveScope_Phase4: [String]? = nil
    public static var TestResolveScope_Phase5: [String]? = nil
    public static var TestResolveScope_Phase6: [String]? = nil
    public static var TestResolveScope_Phase7: [String]? = nil
    public static var TestPreProcessPointers: [String]? = nil
    public static var TestConvertLabelsToPlaceHolders: [String]? = nil
    public static var TestResolveExtendedClass: [String]? = nil
    public static var TestGenerateClassListReorderedByDependancy: [String]? = nil
    public static var TestMergeExtendedClass: [String]? = nil
    public static var TestAddParentConstructorCalls: [String]? = nil
    public static var TestAddConstructorsToStaticClass: [String]? = nil
    public static var TestPreProcessForBlock: [String]? = nil
    public static var TestPreProcessWhileBlock: [String]? = nil
    public static var TestPreProcessIfElseBlock: [String]? = nil
    public static var TestResolveFloatingFunctionCalls: [String]? = nil
    public static var TestConvertCharPlaceHolderToDecimal: [String]? = nil
    public static var TestPreProcessFunctionParams: [String]? = nil
    public static var TestPreProcessExpressions: [String]? = nil
    public static var TestResolveImplicitVarType: [String]? = nil
    public static var TestRestructureClassMemberVars: [String]? = nil
    public static var TestDeployManualIASM: [String]? = nil
    public static var TestConvertStringPlaceHolderToCharDecimalArray: [String]? = nil
    public static var TestMoveTemporaryClassMemberVarToConstructor: [String]? = nil
    public static var TestRemoveNamespaces: [String]? = nil
    public static var TestPrepareClassStructure: [String]? = nil
    public static var TestConvertVarDeclarationsToASM: [String]? = nil
    public static var TestConvertIncDecToOneLine: [String]? = nil
    public static var TestResolveStandAloneIncrementDecrement: [String]? = nil
    public static var TestConvertExpressionsToASM: [String]? = nil
    public static var TestConvertClassVarDeclarationsToPointers: [String]? = nil
    public static var TestConvertReturnToASM: [String]? = nil
    public static var TestConvertInstanceFuncCallAndMemberVarAccessToASM: [String]? = nil
    public static var TestConvertIfStatementToASM: [String]? = nil
    public static var TestConvertFunctionParamsToASM: [String]? = nil
    public static var TestReplaceKeywordsToASM: [String]? = nil
    public static var TestConvertClassFunctionsToLabels: [String]? = nil
    public static var TestGenerateCleanASMCode: [String]? = nil
    public static var TestConvertPointerKeywordsToSymbol: [String]? = nil
    public static var TestAddVMSpecificCode: [String]? = nil
    public static var TestOptimizeRedundantASM: [String]? = nil
    public static var TestMoveREGVarAndVarInitiationToTopOfCode: [String]? = nil
    public static var TestConvertRegToVarCommand: [String]? = nil
    public static var TestRemoveLabelsAndConvertJumpToCodeIndex: [String]? = nil
    public static var TestConvertVariablesToID: [String]? = nil
    public static var TestConvertJumpAndLabelsToNumericID: [String]? = nil
    public static var TestConvertSingleLineOfCodeToByteCode: [UInt8]? = nil
    public static var TestRemoveLabelIDsAndConvertJumpToBinCodeIndex: [UInt8]? = nil
    
    // Commands (8 bit)
    static let NOP: UInt8 = 0x0 // No operation
    static let VMC: UInt8 = 0x1 // Virtual Machine Call
    static let VAR: UInt8 = 0x2 // Declare/Operate on a variable
    static let MOV: UInt8 = 0x3 // Assign value to pointer variable or variable
    static let JMP: UInt8 = 0x4 // Jump to label
    static let JMS: UInt8 = 0x5 // Jump to subroutine
    static let RET: UInt8 = 0x6 // Return from subroutine
    static let ADD: UInt8 = 0x7 // Add
    static let SUB: UInt8 = 0x8 // Subtract
    static let MLT: UInt8 = 0x9 // Multiply
    static let DIV: UInt8 = 0xA // Divide
    static let MOD: UInt8 = 0xB // Modulas
    static let SHL: UInt8 = 0xC // Shift left
    static let SHR: UInt8 = 0xD // Shift right
    static let CAN: UInt8 = 0xE // Conditional Logical AND (&&)
    static let COR: UInt8 = 0xF // Conditional Logical OR (||)
    static let SKE: UInt8 = 0x10 // Skip if equal
    static let AND: UInt8 = 0x11 // Logical AND
    static let LOR: UInt8 = 0x12 // Logical OR
    static let XOR: UInt8 = 0x13 // Exclusive OR
    static let PSH: UInt8 = 0x14 // Push to stack
    static let POP: UInt8 = 0x15 // Pop from stack
    static let LEQ: UInt8 = 0x16 // Logical Operator ==
    static let LNE: UInt8 = 0x17 // Logical Operator !=
    static let LSE: UInt8 = 0x18 // Logical Operator <=
    static let LGE: UInt8 = 0x19 // Logical Operator >=
    static let LGS: UInt8 = 0x1A // Logical Operator <
    static let LGG: UInt8 = 0x1B // Logical Operator >
    static let UMN: UInt8 = 0x1C // UnaryMinus -
    static let UPL: UInt8 = 0x1D // UnaryPlus +
    static let UNT: UInt8 = 0x1E // UnaryNot !
    static let BUC: UInt8 = 0x1F // Bitwise Unary Complement ~

    // Type (8 bit)
    static let TYPE_NULL: UInt8 = 0x0 // null
    static let TYPE_PTR: UInt8 = 0x1 // Operation manipulates on a pointer
    static let TYPE_IMM: UInt8 = 0x2 // Operation manipulates on an immediate value
    static let TYPE_UVAR8: UInt8 = 0x3 // Operation manipulates on 8 bit unsigned variable
    static let TYPE_SVAR8: UInt8 = 0x4 // Operation manipulates on 8 bit signed variable
    static let TYPE_UVAR16: UInt8 = 0x5 // Operation manipulates on 16 bit unsigned variable
    static let TYPE_SVAR16: UInt8 = 0x6 // Operation manipulates on 16 bit signed variable
    static let TYPE_UVAR32: UInt8 = 0x7 // Operation manipulates on 32 bit unsigned variable
    static let TYPE_SVAR32: UInt8 = 0x8 // Operation manipulates on 32 bit signed variable
    static let TYPE_UVAR64: UInt8 = 0x9 // Operation manipulates on 64 bit unsigned variable
    static let TYPE_SVAR64: UInt8 = 0xA // Operation manipulates on 64 bit signed variable
    static let TYPE_UIMM8: UInt8 = 0xB // Operation manipulates on 8 bit unsigned immediate
    static let TYPE_SIMM8: UInt8 = 0xC // Operation manipulates on 8 bit signed immediate
    static let TYPE_UIMM16: UInt8 = 0xD // Operation manipulates on 16 bit unsigned immediate
    static let TYPE_SIMM16: UInt8 = 0xE // Operation manipulates on 16 bit signed immediate
    static let TYPE_UIMM32: UInt8 = 0xF // Operation manipulates on 32 bit unsigned immediate
    static let TYPE_SIMM32: UInt8 = 0x10 // Operation manipulates on 32 bit signed immediate
    static let TYPE_UIMM64: UInt8 = 0x11 // Operation manipulates on 64 bit unsigned immediate
    static let TYPE_SIMM64: UInt8 = 0x12 // Operation manipulates on 64 bit signed immediate
    // OpCode size (Command(1 byte) + OpType(1 byte) = 2 bytes)
    static let MAX_OPCODE_BYTE_SIZE: Int = 2
    // Value always have a signed long size (the size of long may vary by underlying architecture)
    public static var MaxOpcodeValueSize: Int = 0
    // Decimal precision (mutable in compiler)
    static var DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY: Double = 10000

    // Pseudo assembly / placeholder strings

    public static let PH_NOP = "NOP" // No Operation / Label -- If no labels specified, it should be "NOP 0"
    public static let PH_VMC = "VMC" // Virtual Machine Call

    public static let PH_VAR = "VAR" // Use Variable
    public static let PH_VAR8 = "VAR8" // Use 8 bit Variable
    public static let PH_VAR16 = "VAR16" // Use 16 bit Variable
    public static let PH_VAR32 = "VAR32" // Use 32 bit Variable
    public static let PH_VAR64 = "VAR64" // Use 64 bit Variable

    public static let PH_REG = "REG" // Declare/Register variable
    public static let PH_REG8 = "REG8" // Declare/Operate on 8 bit
    public static let PH_REG16 = "REG16" // Declare/Operate on 16 bit
    public static let PH_REG32 = "REG32" // Declare/Operate on 32 bit
    public static let PH_REG64 = "REG64" // Declare/Operate on 64 bit

    public static let PH_MOV = "MOV" // Assign To Variable
    public static let PH_JMP = "JMP" // Jump to label
    public static let PH_JMS = "JMS" // Jump to subroutine
    public static let PH_RET = "RET" // Return from subroutine
    public static let PH_ADD = "ADD" // Add
    public static let PH_SUB = "SUB" // Subtract
    public static let PH_MLT = "MLT" // Multiply
    public static let PH_DIV = "DIV" // Divide
    public static let PH_MOD = "MOD" // Modulas
    public static let PH_SHL = "SHL" // Left shift
    public static let PH_SHR = "SHR" // Right shift
    public static let PH_CAN = "CAN" // Conditional Logical AND (&&)
    public static let PH_COR = "COR" // Conditional Logical OR (||)
    public static let PH_SKE = "SKE" // Skip if equal
    public static let PH_AND = "AND" // Logical AND
    public static let PH_LOR = "LOR" // Logical OR
    public static let PH_XOR = "XOR" // Exclusive OR
    public static let PH_PSH = "PSH" // Push to stack
    public static let PH_POP = "POP" // Pop from stack
    public static let PH_LEQ = "LEQ" // Logical Operator ==
    public static let PH_LNE = "LNE" // Logical Operator !=
    public static let PH_LSE = "LSE" // Logical Operator <=
    public static let PH_LGE = "LGE" // Logical Operator >=
    public static let PH_LGS = "LGS" // Logical Operator <
    public static let PH_LGG = "LGG" // Logical Operator >
    // public static let PH_MAL = "MAL" // Memory allocate
    // public static let PH_DEL = "DEL" // Memory deallocate

    public static let PH_UMN = "UMN" // UnaryMinus -
    public static let PH_UPL = "UPL" // UnaryPlus +
    public static let PH_UNT = "UNT" // UnaryNot !
    public static let PH_BUC = "BUC" // Bitwise Unary Complement ~

    public static let PH_RESV_KW_USING = "using "
    public static let PH_RESV_KW_NAMESPACE = "namespace "
    public static let PH_RESV_KW_CLASS = "class "
    public static let PH_RESV_KW_STATIC = "static "
    public static let PH_RESV_KW_STATIC_CLASS = "static_class "
    public static let PH_RESV_KW_OVERRIDE = "override "
    public static let PH_RESV_KW_VIRTUAL = "virtual "
    public static let PH_RESV_KW_CONST = "const "
    public static let PH_RESV_KW_RETURN = "return "
    public static let PH_RESV_KW_GOTO = "goto "
    public static let PH_RESV_KW_NEW = "new "
    public static let PH_RESV_KW_VOID = "void "
    public static let PH_RESV_KW_IF = "if"
    public static let PH_RESV_KW_ELSE = "else"
    public static let PH_RESV_KW_ELSEIF = "else if"
    public static let PH_RESV_KW_WHILE = "while"
    public static let PH_RESV_KW_BREAK = "break"
    public static let PH_RESV_KW_CONTINUE = "continue"
    public static let PH_RESV_KW_FOR = "for"
    public static let PH_RESV_KW_FUNCTION = "function "
    public static let PH_RESV_KW_ENUM = "enum "

    public static let PH_RESV_KW_FUNCTION_PTR = "function_ptr "
    public static let PH_RESV_KW_FUNCTION_VOID = "function_void "
    public static let PH_RESV_KW_FUNCTION_STRING = "function_string "
    public static let PH_RESV_KW_FUNCTION_CHAR = "function_char "
    public static let PH_RESV_KW_FUNCTION_OBJECT = "function_object "
    public static let PH_RESV_KW_FUNCTION_BOOL = "function_bool "
    public static let PH_RESV_KW_FUNCTION_SBYTE = "function_sbyte "
    public static let PH_RESV_KW_FUNCTION_BYTE = "function_byte "
    public static let PH_RESV_KW_FUNCTION_SHORT = "function_short "
    public static let PH_RESV_KW_FUNCTION_USHORT = "function_ushort "
    public static let PH_RESV_KW_FUNCTION_INT = "function_int "
    public static let PH_RESV_KW_FUNCTION_UINT = "function_uint "
    public static let PH_RESV_KW_FUNCTION_LONG = "function_long "
    public static let PH_RESV_KW_FUNCTION_ULONG = "function_ulong "
    public static let PH_RESV_KW_FUNCTION_FLOAT = "function_float "
    public static let PH_RESV_KW_FUNCTION_DOUBLE = "function_double "
    public static let PH_RESV_KW_FUNCTION_DECIMAL = "function_decimal "

    public static let PH_RESV_KW_STRING = "string "
    public static let PH_RESV_KW_CHAR = "char "
    public static let PH_RESV_KW_PTR = "ptr𒀭 " // Represents a pointer

    public static let PH_RESV_KW_PTR_VOID = "void_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_STRING = "string_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_CHAR = "char_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_OBJECT = "object_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_BOOL = "bool_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_SBYTE = "sbyte_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_BYTE = "byte_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_SHORT = "short_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_USHORT = "ushort_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_INT = "int_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_UINT = "uint_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_LONG = "long_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_ULONG = "ulong_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_FLOAT = "float_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_DOUBLE = "double_" + PH_RESV_KW_PTR
    public static let PH_RESV_KW_PTR_DECIMAL = "decimal_" + PH_RESV_KW_PTR

    public static let PH_RESV_KW_OBJECT = "object " // 64 BIT
    public static let PH_RESV_KW_VAR = "var " // 16 BIT | 32 BIT | 64 BIT | 128 BIT - Default can be changed but range is fixed
    public static let PH_RESV_KW_BOOL = "bool " // 8 BIT | 1 or 0
    public static let PH_RESV_KW_SBYTE = "sbyte " // 8 BIT | -128... 127
    public static let PH_RESV_KW_SHORT = "short " // 16 BIT | -32,768 to +32,767
    public static let PH_RESV_KW_INT = "int " // 32 BIT | -2,147,483,648 to +2,147,483,647
    public static let PH_RESV_KW_LONG = "long " // 64 BIT | -9,223,372,036,854,775,808 to +9,223,372,036,854,775,807
    public static let PH_RESV_KW_ULONG = "ulong " // 64 BIT | 0 to +18,446,744,073,709,551,615
    public static let PH_RESV_KW_BYTE = "byte " // 8 BIT | 0... 255
    public static let PH_RESV_KW_USHORT = "ushort " // 16 BIT | 0 to +65,535
    public static let PH_RESV_KW_UINT = "uint " // 32 BIT | 0 to +4,294,967,295
    public static let PH_RESV_KW_FLOAT = "float " // 32 BIT | ±1.5 x 10−45 ～ ±3.4 x 10^38
    public static let PH_RESV_KW_DOUBLE = "double " // 64 BIT | ±5.0 × 10−324 ～ ±1.7 × 10^308
    public static let PH_RESV_KW_DECIMAL = "decimal " // 128 BIT | ±1.0 x 10-28 ～ ±7.9228 x 10^28
    public static let PH_RESV_KW_BASE = "base"
    public static let PH_RESV_KW_THIS = "this"
    public static let PH_RESV_KW_TRUE = "true"
    public static let PH_RESV_KW_FALSE = "false"
    public static let PH_RESV_KW_NULL = "null"

    public static let PH_RESV_KW_PROGRAM_INIT_POINT_ATTRIBUTE = "[DScript_Init]"
    public static let PH_RESV_KW_PROGRAM_INIT_POINT_LABEL = PH_ID + PH_LABEL + PH_ID + "@DScript_Init;"
    public static let PH_RESV_KW_PROGRAM_MAIN_LOOP_ATTRIBUTE = "[DScript_Main]"
    public static let PH_RESV_KW_PROGRAM_MAIN_LOOP_LABEL = PH_ID + PH_LABEL + PH_ID + "@DScript_Main;"

    public static let PH_RESV_KW_IASM = "__iasm"
    public static let PH_RESV_KW_IASM_FUNCTION_CALL = "DScript.VMC.__iasm"
    public static let PH_RESV_SYSFUNC_MALLOC = "DScript.VMC.Malloc"
    public static let PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE = "DScript.VMC.RegisterAllocatedMemBlockSize"
    public static let PH_RESV_SYSFUNC_SET_FLOAT_PRECISION = "DScript.VMC.SetFloatMultByFactor"
    
    private static var isShowLogs = true
    
    // Obsolete / deprecated keywords
    public static let ObsoleteKeywords: [String] = [
        "public ",
        "private ",
        "protected ",
        PH_RESV_KW_VIRTUAL,
        PH_RESV_KW_STATIC,
        PH_RESV_KW_CONST,
        PH_RESV_KW_ENUM
    ]

    // Class member reserved keywords
    public static let ClassMemberReservedKeyWords: [String] = [
        PH_RESV_KW_VOID,
        PH_RESV_KW_FUNCTION,
        PH_RESV_KW_FUNCTION_PTR,
        PH_RESV_KW_FUNCTION_VOID,
        PH_RESV_KW_FUNCTION_STRING,
        PH_RESV_KW_FUNCTION_CHAR,
        PH_RESV_KW_FUNCTION_OBJECT,
        PH_RESV_KW_FUNCTION_BOOL,
        PH_RESV_KW_FUNCTION_SBYTE,
        PH_RESV_KW_FUNCTION_BYTE,
        PH_RESV_KW_FUNCTION_SHORT,
        PH_RESV_KW_FUNCTION_USHORT,
        PH_RESV_KW_FUNCTION_INT,
        PH_RESV_KW_FUNCTION_UINT,
        PH_RESV_KW_FUNCTION_LONG,
        PH_RESV_KW_FUNCTION_ULONG,
        PH_RESV_KW_FUNCTION_FLOAT,
        PH_RESV_KW_FUNCTION_DOUBLE,
        PH_RESV_KW_FUNCTION_DECIMAL,

        PH_RESV_KW_STRING,
        PH_RESV_KW_CHAR,
        PH_RESV_KW_VAR,
        PH_RESV_KW_PTR,

        PH_RESV_KW_PTR_VOID,
        PH_RESV_KW_PTR_STRING,
        PH_RESV_KW_PTR_CHAR,
        PH_RESV_KW_PTR_OBJECT,
        PH_RESV_KW_PTR_BOOL,
        PH_RESV_KW_PTR_SBYTE,
        PH_RESV_KW_PTR_BYTE,
        PH_RESV_KW_PTR_SHORT,
        PH_RESV_KW_PTR_USHORT,
        PH_RESV_KW_PTR_INT,
        PH_RESV_KW_PTR_UINT,
        PH_RESV_KW_PTR_LONG,
        PH_RESV_KW_PTR_ULONG,
        PH_RESV_KW_PTR_FLOAT,
        PH_RESV_KW_PTR_DOUBLE,
        PH_RESV_KW_PTR_DECIMAL,

        PH_RESV_KW_OBJECT,

        PH_RESV_KW_BOOL,
        PH_RESV_KW_SBYTE,
        PH_RESV_KW_SHORT,
        PH_RESV_KW_INT,
        PH_RESV_KW_LONG,
        PH_RESV_KW_ULONG,
        PH_RESV_KW_BYTE,
        PH_RESV_KW_USHORT,
        PH_RESV_KW_UINT,
        PH_RESV_KW_FLOAT,
        PH_RESV_KW_DOUBLE,
        PH_RESV_KW_DECIMAL
    ]

    // All reserved keywords
    public static let AllReservedKeywords: [String] = [
        PH_RESV_KW_USING,
        PH_RESV_KW_NAMESPACE,
        PH_RESV_KW_CLASS,
        PH_RESV_KW_STATIC_CLASS,
        PH_RESV_KW_OVERRIDE,
        PH_RESV_KW_VIRTUAL,
        PH_RESV_KW_CONST,
        PH_RESV_KW_ENUM,
        PH_RESV_KW_RETURN,
        PH_RESV_KW_GOTO,
        PH_RESV_KW_NEW,

        PH_RESV_KW_VOID,
        PH_RESV_KW_IF,
        PH_RESV_KW_ELSE,
        PH_RESV_KW_ELSEIF,
        PH_RESV_KW_WHILE,
        PH_RESV_KW_BREAK,
        PH_RESV_KW_CONTINUE,
        PH_RESV_KW_FOR,

        PH_RESV_KW_FUNCTION,
        PH_RESV_KW_FUNCTION_PTR,
        PH_RESV_KW_FUNCTION_VOID,
        PH_RESV_KW_FUNCTION_STRING,
        PH_RESV_KW_FUNCTION_CHAR,
        PH_RESV_KW_FUNCTION_OBJECT,
        PH_RESV_KW_FUNCTION_BOOL,
        PH_RESV_KW_FUNCTION_SBYTE,
        PH_RESV_KW_FUNCTION_BYTE,
        PH_RESV_KW_FUNCTION_SHORT,
        PH_RESV_KW_FUNCTION_USHORT,
        PH_RESV_KW_FUNCTION_INT,
        PH_RESV_KW_FUNCTION_UINT,
        PH_RESV_KW_FUNCTION_LONG,
        PH_RESV_KW_FUNCTION_ULONG,
        PH_RESV_KW_FUNCTION_FLOAT,
        PH_RESV_KW_FUNCTION_DOUBLE,
        PH_RESV_KW_FUNCTION_DECIMAL,

        PH_RESV_KW_STRING,
        PH_RESV_KW_CHAR,
        PH_RESV_KW_VAR,
        PH_RESV_KW_PTR,

        PH_RESV_KW_PTR_VOID,
        PH_RESV_KW_PTR_STRING,
        PH_RESV_KW_PTR_CHAR,
        PH_RESV_KW_PTR_OBJECT,
        PH_RESV_KW_PTR_BOOL,
        PH_RESV_KW_PTR_SBYTE,
        PH_RESV_KW_PTR_BYTE,
        PH_RESV_KW_PTR_SHORT,
        PH_RESV_KW_PTR_USHORT,
        PH_RESV_KW_PTR_INT,
        PH_RESV_KW_PTR_UINT,
        PH_RESV_KW_PTR_LONG,
        PH_RESV_KW_PTR_ULONG,
        PH_RESV_KW_PTR_FLOAT,
        PH_RESV_KW_PTR_DOUBLE,
        PH_RESV_KW_PTR_DECIMAL,

        PH_RESV_KW_OBJECT,

        PH_RESV_KW_BOOL,
        PH_RESV_KW_SBYTE,
        PH_RESV_KW_SHORT,
        PH_RESV_KW_INT,
        PH_RESV_KW_LONG,
        PH_RESV_KW_BYTE,
        PH_RESV_KW_USHORT,
        PH_RESV_KW_UINT,
        PH_RESV_KW_FLOAT,
        PH_RESV_KW_DOUBLE,
        PH_RESV_KW_DECIMAL,

        PH_RESV_KW_BASE,
        PH_RESV_KW_THIS,
        PH_RESV_KW_TRUE,
        PH_RESV_KW_FALSE,
        PH_RESV_KW_NULL,
        PH_RESV_KW_IASM
    ]

    // All primitive type pointer keywords
    public static let AllPrimitiveTypePointerKeywords: [String] = [
        PH_RESV_KW_PTR,

        PH_RESV_KW_PTR_VOID,
        PH_RESV_KW_PTR_STRING,
        PH_RESV_KW_PTR_CHAR,
        PH_RESV_KW_PTR_OBJECT,
        PH_RESV_KW_PTR_BOOL,
        PH_RESV_KW_PTR_SBYTE,
        PH_RESV_KW_PTR_BYTE,
        PH_RESV_KW_PTR_SHORT,
        PH_RESV_KW_PTR_USHORT,
        PH_RESV_KW_PTR_INT,
        PH_RESV_KW_PTR_UINT,
        PH_RESV_KW_PTR_LONG,
        PH_RESV_KW_PTR_ULONG,
        PH_RESV_KW_PTR_FLOAT,
        PH_RESV_KW_PTR_DOUBLE,
        PH_RESV_KW_PTR_DECIMAL
    ]

    // All function keywords
    public static let AllFunctionKeywords: [String] = [
        PH_RESV_KW_FUNCTION_PTR,
        PH_RESV_KW_FUNCTION_VOID,
        PH_RESV_KW_FUNCTION_STRING,
        PH_RESV_KW_FUNCTION_CHAR,
        PH_RESV_KW_FUNCTION_OBJECT,
        PH_RESV_KW_FUNCTION_BOOL,
        PH_RESV_KW_FUNCTION_SBYTE,
        PH_RESV_KW_FUNCTION_BYTE,
        PH_RESV_KW_FUNCTION_SHORT,
        PH_RESV_KW_FUNCTION_USHORT,
        PH_RESV_KW_FUNCTION_INT,
        PH_RESV_KW_FUNCTION_UINT,
        PH_RESV_KW_FUNCTION_LONG,
        PH_RESV_KW_FUNCTION_ULONG,
        PH_RESV_KW_FUNCTION_FLOAT,
        PH_RESV_KW_FUNCTION_DOUBLE,
        PH_RESV_KW_FUNCTION_DECIMAL
    ]

    // Keywords that cannot declare variables
    public static let NonVariableDeclareKeywords: [String] = [
        PH_RESV_KW_USING,
        PH_RESV_KW_NAMESPACE,
        PH_RESV_KW_CLASS,
        PH_RESV_KW_STATIC_CLASS,
        PH_RESV_KW_OVERRIDE,
        PH_RESV_KW_VIRTUAL,
        PH_RESV_KW_CONST,
        PH_RESV_KW_ENUM,
        PH_RESV_KW_RETURN,
        PH_RESV_KW_GOTO,
        PH_RESV_KW_NEW,
        PH_RESV_KW_VOID,

        PH_RESV_KW_FUNCTION,
        PH_RESV_KW_FUNCTION_PTR,
        PH_RESV_KW_FUNCTION_VOID,
        PH_RESV_KW_FUNCTION_STRING,
        PH_RESV_KW_FUNCTION_CHAR,
        PH_RESV_KW_FUNCTION_OBJECT,
        PH_RESV_KW_FUNCTION_BOOL,
        PH_RESV_KW_FUNCTION_SBYTE,
        PH_RESV_KW_FUNCTION_BYTE,
        PH_RESV_KW_FUNCTION_SHORT,
        PH_RESV_KW_FUNCTION_USHORT,
        PH_RESV_KW_FUNCTION_INT,
        PH_RESV_KW_FUNCTION_UINT,
        PH_RESV_KW_FUNCTION_LONG,
        PH_RESV_KW_FUNCTION_ULONG,
        PH_RESV_KW_FUNCTION_FLOAT,
        PH_RESV_KW_FUNCTION_DOUBLE,
        PH_RESV_KW_FUNCTION_DECIMAL,

        PH_RESV_KW_IF,
        PH_RESV_KW_ELSE,
        PH_RESV_KW_ELSEIF,
        PH_RESV_KW_WHILE,
        PH_RESV_KW_BREAK,
        PH_RESV_KW_CONTINUE,
        PH_RESV_KW_FOR,
        PH_RESV_KW_TRUE,
        PH_RESV_KW_FALSE,
        PH_RESV_KW_NULL,
        PH_RESV_KW_IASM
    ]

    // Keywords that are not methods
    public static let NonMethodKeywords: [String] = [
        PH_RESV_KW_IF,
        PH_RESV_KW_ELSEIF,
        PH_RESV_KW_ELSE,
        PH_RESV_KW_WHILE,
        PH_RESV_KW_BREAK,
        PH_RESV_KW_CONTINUE,
        PH_RESV_KW_FOR,
        PH_RESV_KW_TRUE,
        PH_RESV_KW_FALSE,
        PH_RESV_KW_NULL,
        PH_RESV_KW_IASM
    ]

    // Keywords that has round brackets and can hold scope brackets (curly brackets) that can be ommitted
    public static let RoundBracketScopeHolderKeywords: [String] = [
        PH_RESV_KW_IF,
        PH_RESV_KW_ELSEIF,
        PH_RESV_KW_WHILE,
        PH_RESV_KW_FOR
    ]
    
    public static let PH_ID = "𒀭"
    public static let PH_POINTER = "PH_PTR"
    public static let PH_ADDRESS = "PH_ADDRESS"
    public static let PH_EXP_STR = "PH_EXP_STR"
    public static let PH_STR = "PH_STR"
    public static let PH_CHAR = "PH_CHAR"
    public static let PH_NEW = "PH_NEW"
    public static let PH_LABEL = "PH_LBL"
    public static let PH_LEFTEXP_RESULT = "PH_LEFTEXP"
    public static let PH_ESC_DBL_QUOTE = PH_ID + "PH_ESC_DBL_QUOTE" + PH_ID
    public static let PH_ESC_SNGL_QUOTE = PH_ID + "PH_ESC_SNGL_QUOTE" + PH_ID

    // These are used for temporary calculation variables used in VM
    public static let REG_NULL = "REG_NULL"
    public static let REG_SVAR0 = "REG_SVAR0"
    public static let REG_SVAR1 = "REG_SVAR1"
    public static let REG_UVAR0 = "REG_UVAR0"
    public static let REG_UVAR1 = "REG_UVAR1"

    public static let REG_VAR_BYTE0 = "REG_VAR_BYTE0"
    public static let REG_VAR_BYTE1 = "REG_VAR_BYTE1"
    public static let REG_VAR_SBYTE0 = "REG_VAR_SBYTE0"
    public static let REG_VAR_SBYTE1 = "REG_VAR_SBYTE1"
    public static let REG_VAR_SHORT0 = "REG_VAR_SHORT0"
    public static let REG_VAR_SHORT1 = "REG_VAR_SHORT1"
    public static let REG_VAR_USHORT0 = "REG_VAR_USHORT0"
    public static let REG_VAR_USHORT1 = "REG_VAR_USHORT1"
    public static let REG_VAR_INT0 = "REG_VAR_INT0"
    public static let REG_VAR_INT1 = "REG_VAR_INT1"
    public static let REG_VAR_UINT0 = "REG_VAR_UINT0"
    public static let REG_VAR_UINT1 = "REG_VAR_UINT1"
    public static let REG_VAR_LONG0 = "REG_VAR_LONG0"
    public static let REG_VAR_LONG1 = "REG_VAR_LONG1"
    public static let REG_VAR_ULONG0 = "REG_VAR_ULONG0"
    public static let REG_VAR_ULONG1 = "REG_VAR_ULONG1"
    public static let REG_VAR_FLOAT0 = "REG_VAR_FLOAT0"
    public static let REG_VAR_FLOAT1 = "REG_VAR_FLOAT1"
    public static let REG_VAR_DOUBLE0 = "REG_VAR_DOUBLE0"
    public static let REG_VAR_DOUBLE1 = "REG_VAR_DOUBLE1"
    public static let REG_VAR_DECIMAL0 = "REG_VAR_DECIMAL0"
    public static let REG_VAR_DECIMAL1 = "REG_VAR_DECIMAL1"

    // Targetted for x bit processor
    public static var TARGET_ARCH_BIT_SIZE: Int = 64
    public static var CHAR_BIT_SIZE: Int = 16
    public static var BYTE_BIT_SIZE: Int = 8
    public static var SHORT_BIT_SIZE: Int = 16
    public static var INT_BIT_SIZE: Int = 32
    public static var LONG_BIT_SIZE: Int = 64
    public static var FLOAT_BIT_SIZE: Int = 32
    public static var DOUBLE_BIT_SIZE: Int = 64
    public static var DECIMAL_BIT_SIZE: Int = 64 // Could be 128 bit, if supported

    // The string length to use when generating random alphabetic strings
    public static let RND_ALPHABET_STRING_LEN_MAX = 10
    // Temporary variable identifier to attach to the randomized string
    // (we will use this identifier to differentiate between temporary variables and user-defined variables)
    public static let TmpVarIdentifier = "RQ20131212DSRNDID"

    // List of operators (order matters!)
    public static let Operators: [String] = [
        "UnaryMinus", "UnaryPlus", "UnaryNot", "BitwiseUnaryComplement",
        "＋", "—",
        "<=", ">=", "<", ">", "==", "!=",
        "&&", "||", "&", "|", "^",
        "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=",
        "<<", ">>",
        "=", "!", "~", "--", "++",
        "-", "+", "*", "/", "%", "."
    ]

    // List of code separator tokens
    private static let SeparatorPattern =
        #"([＋—*()\^\/{}=;,:<>!&\|%~]|(?<!E)[\+\-]|"# + PH_RESV_KW_NEW + #")"#
    
    // Stores all literals
    public static var StringLiteralList: [String: String] = [:]
    public static var CharLiteralList: [String: String] = [:]

    // Stores the actual member size list of a non-static class
    private static var ActualClassMemberVarSizeList: [String: [Int]] = [:]

    // Stores the actual var type of a variable before converted into a pointer type
    private static var ActualClassMemberPointerVarTypeList: [String: String] = [:]

    // Stores all the "using" namespace declarations
    private static var AllUsingNamespaceDeclarations: [String] = []
    
    // MARK: - Helpers
    private static func ShowLog(_ text: String) {
        if self.isShowLogs { print(text) }
    }

    /// <summary>
    /// Generates a random string with a given size.
    /// </summary>
    /// <param name="size">String length of random string</param>
    /// <param name="lowerCase">Whether to include lower case or not</param>
    /// <param name="additionalIdentifier">Additional string that can be added to the end of string</param>
    /// <returns></returns>
    public static func RandomString(_ size: Int, _ lowerCase: Bool, _ additionalIdentifier: String) -> String {
        var builder = ""

        // Unicode/ASCII Letters are divided into two blocks
        // (Letters 65–90 / 97–122):
        // The first group containing the uppercase letters and
        // the second group containing the lowercase.

        let offset: UInt32 = lowerCase ? UnicodeScalar("a").value : UnicodeScalar("A").value
        let lettersOffset: UInt32 = 26 // A...Z or a..z: length=26

        // NOTE: C# Random is time-seeded per call; replicate behavior
        var random = SystemRandomNumberGenerator()

        for _ in 0..<size {
            let rand = Int(
                offset + UInt32.random(
                    in: 0..<lettersOffset,
                    using: &random
                )
            )
            if let scalar = UnicodeScalar(rand) {
                builder.append(Character(scalar))
            }
        }

        var ret = lowerCase ? builder.lowercased() : builder
        ret += additionalIdentifier
        return ret
    }

    /// <summary>
    /// Cleans up string by removing sequential spaces, tabs, linebreaks
    /// </summary>
    /// <param name="text">Context string</param>
    /// <returns>Cleaned string</returns>
    public static func Cleanup(_ text: String) -> String {
        var text = text
        text = ReplaceSequentialRepStringToSingle(text, "\r\n")
        text = ReplaceSequentialRepStringToSingle(text, " ")
        text = ReplaceSequentialRepStringToSingle(text, "\t")
        text = text.trimmingCharacters(in: .whitespacesAndNewlines)
        return text
    }

    /// <summary>
    /// Replaces sequential occurance of a string to a single string
    /// </summary>
    /// <param name="text">Context string</param>
    /// <param name="replaceToSingleString">Specify the 'single' string you are after</param>
    /// <returns></returns>
    public static func ReplaceSequentialRepStringToSingle(_ text: String, _ replaceToSingleString: String) -> String {
        var text = text
        var oldLength: Int

        repeat {
            oldLength = text.count
            text = text.replacingOccurrences(
                of: replaceToSingleString + replaceToSingleString,
                with: replaceToSingleString
            )
        } while text.count != oldLength

        return text
    }

    /// <summary>Replace first occurance of string</summary>
    /// <param name="text">Context string</param>
    /// <param name="search">String to search for</param>
    /// <param name="replace">String to be replaced</param>
    /// <returns></returns>
    public static func ReplaceFirstOccurance(_ text: String, _ search: String, _ replace: String) -> String {
        // C# compatibility: IndexOf("") == 0
        if search.isEmpty {
            return replace + text
        }
        guard let range = text.range(of: search) else {
            return text
        }
        return text.replacingCharacters(in: range, with: replace)
    }

    
    public static func Compile(
        _ co: CompileOptions,
        _ isWriteToBinFile: Bool = true,
        _ isShowLogs: Bool = true,
    ) -> [UInt8]? {

        self.isShowLogs = isShowLogs
        
        // --- File existence check ---
        if !FileManager.default.fileExists(atPath: co.FileName) {
            ShowLog("File " + co.FileName + " does not exist.")
            return nil
        }

        // --- Define / Set architecture bit size ---
        if co.MaxBitSize == 16 {
            TARGET_ARCH_BIT_SIZE = 16
            CHAR_BIT_SIZE = 16
            BYTE_BIT_SIZE = 8
            SHORT_BIT_SIZE = 16
            INT_BIT_SIZE = 16
            LONG_BIT_SIZE = 16
            FLOAT_BIT_SIZE = 16
            DOUBLE_BIT_SIZE = 16
            DECIMAL_BIT_SIZE = 16

        } else if co.MaxBitSize == 32 {
            TARGET_ARCH_BIT_SIZE = 32
            CHAR_BIT_SIZE = 16
            BYTE_BIT_SIZE = 8
            SHORT_BIT_SIZE = 16
            INT_BIT_SIZE = 32
            LONG_BIT_SIZE = 32
            FLOAT_BIT_SIZE = 32
            DOUBLE_BIT_SIZE = 32
            DECIMAL_BIT_SIZE = 32

        } else if co.MaxBitSize == 64 {
            TARGET_ARCH_BIT_SIZE = 64
            CHAR_BIT_SIZE = 16
            BYTE_BIT_SIZE = 8
            SHORT_BIT_SIZE = 16
            INT_BIT_SIZE = 32
            LONG_BIT_SIZE = 64
            FLOAT_BIT_SIZE = 32
            DOUBLE_BIT_SIZE = 64
            DECIMAL_BIT_SIZE = 64

        } else if co.MaxBitSize == 128 {
            TARGET_ARCH_BIT_SIZE = 128
            CHAR_BIT_SIZE = 16
            BYTE_BIT_SIZE = 8
            SHORT_BIT_SIZE = 16
            INT_BIT_SIZE = 32
            LONG_BIT_SIZE = 64
            FLOAT_BIT_SIZE = 32
            DOUBLE_BIT_SIZE = 64
            DECIMAL_BIT_SIZE = 128
        }

        // --- Opcode value size ---
        MaxOpcodeValueSize = co.MaxBitSize / 8

        // --- Decimal precision ---
        DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY =
            pow(10.0, Double(co.DecimalPointPrecision))

        // --- Logging ---
        ShowLog("Compiling " + URL(fileURLWithPath: co.FileName).lastPathComponent)

        // --- Read script file ---
        var code = try! String(contentsOfFile: co.FileName, encoding: .utf8)

        // Normalize line breaks to \n
        code = code
            .replacingOccurrences(of: "\r\n", with: "\n")
            .replacingOccurrences(of: "\r", with: "\n")
        
        ShowLog("Loading external includes...")

        // --- Load external includes (using xxx;) ---
        let dirPath =
            URL(fileURLWithPath: co.FileName)
                .deletingLastPathComponent()
                .path

        code = LoadExternalIncludes(dirPath, code)

        // Test
        if IsGenerateUnitTestFiles { TestLoadExternalIncludesString = code }
        
        ShowLog("Preprocessing... ")
        // Preprocess script code and convert them into line of codes
        var codes = PreProcessCleanUp(code)
        
        ShowLog("Converting to assembly... ")
        // Converts preprocessed intermediatary source code to Assembly language codes
        codes = ConvertIntermCodeToAsm(codes, co)
        
        ShowLog("Converting to binary... ")
        // Convert IASM codes to binary and write to file
        let binCode = ConvertAsmToByteCode(codes)
        
        // Write final binary code to file
        if isWriteToBinFile {
            ShowLog("Generating binary file... ")

            let url = URL(fileURLWithPath: co.BinFileName)

            do {
                let data = Data(binCode)
                try data.write(to: url, options: .atomic)
            } catch {
                ShowLog("Failed to write binary file: \(error)")
                return nil
            }
        }
        
        ShowLog("Done!")
        
        return binCode
    }
    
    /// <summary>
    /// Cleans up string by removing sequential spaces, tabs, linebreaks
    /// and single line/block comments // /* */
    /// </summary>
    /// <param name="text">Context string</param>
    /// <returns>Cleaned string</returns>
    public static func CleanupCode(_ input: String) -> String {
        let blockComments = #"/\*(.*?)\*/"#
        let lineComments = #"//(.*?)\r?\n"#
        let strings = #""((\\[^\n]|[^"\n])*)""#
        let verbatimStrings = #"@("[^"]*")+"#

        // C# と同じ：末尾に改行を追加
        var text = input + "\n"

        let pattern = "\(blockComments)|\(lineComments)|\(strings)|\(verbatimStrings)"
        let regex = try! NSRegularExpression(
            pattern: pattern,
            options: [.dotMatchesLineSeparators]
        )

        let nsText = text as NSString
        let fullRange = NSRange(location: 0, length: nsText.length)

        // 置換内容を一旦収集（後ろから置換するため）
        var replacements: [(NSRange, String)] = []

        regex.enumerateMatches(in: text, options: [], range: fullRange) { match, _, _ in
            guard let match = match else { return }

            let matchedString = nsText.substring(with: match.range)

            if matchedString.hasPrefix("/*") {
                // block comment → 削除
                replacements.append((match.range, ""))
            } else if matchedString.hasPrefix("//") {
                // line comment → 改行1つ
                replacements.append((match.range, "\n"))
            } else {
                // 文字列リテラルは保持
                replacements.append((match.range, matchedString))
            }
        }

        // 後ろから置換（Range 崩壊防止）
        for (range, replacement) in replacements.reversed() {
            text = (text as NSString).replacingCharacters(in: range, with: replacement)
        }

        // C# と同じ後処理
        text = text.replacingOccurrences(of: "\t", with: " ")
        text = text.replacingOccurrences(of: "\r\n", with: "")
        text = text.replacingOccurrences(of: "\n", with: "")

        return text
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
    public static func ConvertLiteralsToPlaceholders(_ input: String) -> String {
        var code = input

        // --- String literals ---
        let strRegex = try! NSRegularExpression(pattern: #""(.*?)""#, options: [])
        let nsCode = code as NSString
        let strMatches = strRegex.matches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: nsCode.length)
        )

        for i in 0..<strMatches.count {
            let match = strMatches[i]
            let strValueRaw = nsCode.substring(with: match.range)

            // Replace first occurance with placeholder
            let placeholder = PH_ID + PH_STR + String(i) + PH_ID
            code = ReplaceFirstOccurance(code, strValueRaw, placeholder)

            // Restore escaped placeholders before storing
            var strValue = strValueRaw
            strValue = strValue.replacingOccurrences(of: PH_ESC_DBL_QUOTE, with: "\"")
            strValue = strValue.replacingOccurrences(of: PH_ESC_SNGL_QUOTE, with: "'")

            StringLiteralList[placeholder] = strValue
        }

        // --- Char literals ---
        let charRegex = try! NSRegularExpression(pattern: #"'(.*?)'"#, options: [])
        let nsCode2 = code as NSString
        let charMatches = charRegex.matches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: nsCode2.length)
        )

        for i in 0..<charMatches.count {
            let match = charMatches[i]
            let charValueRaw = nsCode2.substring(with: match.range)

            // Replace first occurance with placeholder
            let placeholder = PH_ID + PH_CHAR + String(i) + PH_ID
            code = ReplaceFirstOccurance(code, charValueRaw, placeholder)

            // Restore escaped placeholders before storing
            var charValue = charValueRaw
            charValue = charValue.replacingOccurrences(of: PH_ESC_DBL_QUOTE, with: "\"")
            charValue = charValue.replacingOccurrences(of: PH_ESC_SNGL_QUOTE, with: "'")

            CharLiteralList[placeholder] = charValue
        }

        return code
    }
    
    /// <summary>
    /// Convert increment/decrement operators to placeholders
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static func ConvertIncDecOperatorsToPlaceholders(_ input: String) -> String {
        var code = input
        var operatorList = Operators
        let additionalSymbols = [";", ",", ")", "(", "[", "]", "{", "}"]
        operatorList.insert(contentsOf: additionalSymbols, at: 0)
        
        // Insert space after all operators
        for op in operatorList {
            // Ignore plus + and minus -
            if op == "+" || op == "-" || op == "++" || op == "--" {
                continue
            }
            code = code.replacingOccurrences(of: op, with: " \(op) ")
        }
        // Convert ++ and -- to placeholders
        code = code.replacingOccurrences(of: " ++", with: "＋")
        code = code.replacingOccurrences(of: "++ ", with: "＋")
        code = code.replacingOccurrences(of: " --", with: "—")
        code = code.replacingOccurrences(of: "-- ", with: "—")

        // Revert back empty spaces
        for op in operatorList {
            // Ignore plus + and minus -
            if op == "+" || op == "-" || op == "++" || op == "--" {
                continue
            }
            code = code.replacingOccurrences(of: " \(op) ", with: op)
        }
        
        return code;
    }

    /// <summary>enum resolver</summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static func ResolveEnum(_ code: String) -> String {
        var code = code

        // Regex: " enum (.*?)}" with Singleline
        let pattern = #" \#(PH_RESV_KW_ENUM)(.*?)\}"#
        let regex = try! NSRegularExpression(
            pattern: pattern,
            options: [.dotMatchesLineSeparators]
        )

        let matches = regex.matches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: code.utf16.count)
        )

        // IMPORTANT:
        // Apply replacements from the back to avoid range invalidation.
        // This mirrors C#'s string.Replace safety when multiple enums exist.
        for match in matches.reversed() {
            guard let matchRange = Range(match.range, in: code) else { continue }
            let matchedText = String(code[matchRange])

            var enumCounter = 0

            // Replace " enum " → " static class "
            let newCode = matchedText.replacingOccurrences(
                of: " " + PH_RESV_KW_ENUM,
                with: " " + PH_RESV_KW_STATIC + PH_RESV_KW_CLASS
            )

            // Extract enum body { ... }
            let bodyRegex = try! NSRegularExpression(
                pattern: #"\{(.*?)\}"#,
                options: [.dotMatchesLineSeparators]
            )

            guard
                let bodyMatch = bodyRegex.firstMatch(
                    in: newCode,
                    options: [],
                    range: NSRange(location: 0, length: newCode.utf16.count)
                ),
                let bodyRange = Range(bodyMatch.range, in: newCode)
            else { continue }

            let body = String(newCode[bodyRange])
                .replacingOccurrences(of: "{", with: "")
                .replacingOccurrences(of: "}", with: "")

            let enumList = body.split(separator: ",", omittingEmptySubsequences: false)

            var results = ""

            for rawItem in enumList {
                let item = rawItem.trimmingCharacters(in: .whitespacesAndNewlines)
                if item.isEmpty { continue }

                if item.contains("=") {
                    // Has pre-assigned value
                    let valueRegex = try! NSRegularExpression(pattern: #"=.*?(\d+)$"#)
                    let valueMatches = valueRegex.matches(
                        in: item,
                        options: [],
                        range: NSRange(location: 0, length: item.utf16.count)
                    )

                    if
                        let m = valueMatches.first,
                        let r = Range(m.range(at: 1), in: item),
                        let v = Int(item[r])
                    {
                        enumCounter = v + 1
                    }

                    results +=
                        "public "
                        + PH_RESV_KW_STATIC
                        + PH_RESV_KW_INT
                        + item
                        + ";"
                        + "\n"
                } else {
                    results +=
                        "public "
                        + PH_RESV_KW_STATIC
                        + PH_RESV_KW_INT
                        + item
                        + "="
                        + String(enumCounter)
                        + ";"
                        + "\n"
                    enumCounter += 1
                }
            }

            // Replace enum body with generated static fields
            let replaced = bodyRegex.stringByReplacingMatches(
                in: newCode,
                options: [],
                range: NSRange(location: 0, length: newCode.utf16.count),
                withTemplate: "{\(results)}"
            )

            code = code.replacingOccurrences(of: matchedText, with: replaced)
        }

        return code
    }
    
    /// <summary>
    /// Since we do not support parameters for constructors, to support them,
    /// we need to generate a constructor without parameters and call
    /// a specifically generated constructor method from the constructor
    /// </summary>
    public static func ResolveConstructors(_ input: String) -> String {
        var code = input

        // Match: class XXX {
        let classRegex = try! NSRegularExpression(
            pattern: #"\#(PH_RESV_KW_CLASS)(.*?)\{"#,
            options: [.dotMatchesLineSeparators]
        )

        let classMatches = classRegex.matches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: code.utf16.count)
        )

        // IMPORTANT: iterate from back to front (mirrors C# string.Replace safety)
        for m in classMatches.reversed() {
            guard let classRange = Range(m.range, in: code) else { continue }

            // Extract class name
            var className = String(code[classRange])
                .replacingOccurrences(of: PH_RESV_KW_CLASS, with: "")
                .replacingOccurrences(of: "{", with: "")
                .trimmingCharacters(in: .whitespacesAndNewlines)

            // Remove inheritance (class A : B)
            let inheritRegex = try! NSRegularExpression(pattern: ":(.*?)$")
            className = inheritRegex
                .stringByReplacingMatches(
                    in: className,
                    options: [],
                    range: NSRange(location: 0, length: className.utf16.count),
                    withTemplate: ""
                )
                .trimmingCharacters(in: .whitespacesAndNewlines)

            // Find constructor in *current full code*
            let ctorRegex = try! NSRegularExpression(
                pattern: "public " + className + #"\((.*?)\)"#,
                options: [.dotMatchesLineSeparators]
            )

            let ctorMatches = ctorRegex.matches(
                in: code,
                options: [],
                range: NSRange(location: 0, length: code.utf16.count)
            )

            // C# behavior: only process first constructor if it exists
            guard let ctorMatch = ctorMatches.first,
                  let ctorRange = Range(ctorMatch.range, in: code)
            else {
                continue
            }

            let constructor = String(code[ctorRange])

            // Always generate:
            // public ClassName(){}
            // public void ClassName_Constructor(...)
            let newConstructorCode =
                "public \(className)(){} " +
                constructor.replacingOccurrences(
                    of: "public \(className)",
                    with: "public void \(className)_Constructor"
                )

            code = code.replacingOccurrences(
                of: constructor,
                with: newConstructorCode
            )
        }

        return code
    }
    
    /// <summary>
    /// Remove obsolete keywords that are only used for compatibility reasons with other languages
    /// </summary>
    /// <param name="code"></param>
    /// <param name="keywordList"></param>
    /// <returns></returns>
    public static func ReplaceKeywords(
        _ input: String,
        _ keywordList: [String]
    ) -> String {
        var code = input

        // 1. Replace "static class" → "static_class"
        // C# uses Regex.Replace
        let staticClassRegex = try! NSRegularExpression(
            pattern: PH_RESV_KW_STATIC + PH_RESV_KW_CLASS,
            options: []
        )
        code = staticClassRegex.stringByReplacingMatches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: code.utf16.count),
            withTemplate: PH_RESV_KW_STATIC_CLASS
        )

        // 2. Replace (float) / (double) casts
        // C# string.Replace → Swift replacingOccurrences
        // 2. Replace (float) / (double) casts
        let multiply = String(Int(DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY)) + "*"
        code = code.replacingOccurrences(of: "(float)", with: multiply)
        code = code.replacingOccurrences(of: "(double)", with: multiply)

        // 3. Remove obsolete keywords
        for s in keywordList {
            code = code.replacingOccurrences(of: s, with: "")
        }

        return code
    }
    
    /// <summary>
    /// Apply further optimizations for pre-processing code separations
    /// </summary>
    public static func SanitizeKeywordForSeparation(_ input: String) -> String {
        var code = input

        let pattern = PH_RESV_KW_RETURN + #"(.*?);"#
        let regex = try! NSRegularExpression(pattern: pattern, options: [])

        code = regex.stringByReplacingMatches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: code.utf16.count),
            withTemplate: PH_RESV_KW_RETURN + "($1);"
        )

        return code
    }
    
    /// <summary>
    /// Convert "new" keywords to placeholders
    /// </summary>
    public static func ConvertNewKeywordToPlaceholders(_ input: String) -> String {
        var code = input

        let pattern = #"(?<=[^a-zA-Z0-9_])new "#
        let regex = try! NSRegularExpression(pattern: pattern, options: [])

        code = regex.stringByReplacingMatches(
            in: code,
            options: [],
            range: NSRange(location: 0, length: code.utf16.count),
            withTemplate: PH_ID + PH_NEW + PH_ID
        )

        return code
    }
    
    /// <summary>
    /// Preprocesses the raw string of codes before separating it to lines of code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static func PreProcessCodeBeforeLOCSeparation(_ input: String) -> String {
        var code = input

        // Convert escape characters \" to placeholders
        code = code.replacingOccurrences(of: "\\\"", with: PH_ESC_DBL_QUOTE)
        code = code.replacingOccurrences(
            of: "'\"'",
            with: "'" + PH_ESC_DBL_QUOTE + "'"
        )
        // Convert single quote within a char '\'' to placeholder
        code = code.replacingOccurrences(of: "\\'", with: PH_ESC_SNGL_QUOTE)
        // Remove line breaks, tabs, comments and comment blocks
        code = CleanupCode(code)
        // Convert string literals to placeholders
        code = ConvertLiteralsToPlaceholders(code)
        // Convert increment/decrement operators to placeholders
        code = ConvertIncDecOperatorsToPlaceholders(code)
        // Remove multiple spaces to single space
        code = ReplaceSequentialRepStringToSingle(code, " ")
        // Resolve enum keywords and convert them to plain const int values
        code = ResolveEnum(code)
        // Generate new constructors to allow parameters
        code = ResolveConstructors(code)
        // Remove obsolete keywords that are only used for compatibility reasons
        code = ReplaceKeywords(code, ObsoleteKeywords)
        // Apply further optimizations for pre-processing code separations
        code = SanitizeKeywordForSeparation(code)
        // Convert "new" keywords to placeholders
        code = ConvertNewKeywordToPlaceholders(code)

        return code
    }
    
    /// <summary>
    /// Reads all the source code specified by the "using " keyword
    /// and merges together with our main code
    /// </summary>
    /// <param name="currentFilePath"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static func LoadExternalIncludes(
        _ currentFilePath: String,
        _ inputCode: String
    ) -> String {

        var code = inputCode
        var LoadedFileList: [String] = []

        // tmpCode stores pre-cleaned up source code so we can search for the "using " keyword
        var tmpCode = PreProcessCodeBeforeLOCSeparation(code)
        CharLiteralList.removeAll()
        StringLiteralList.removeAll()

        let pattern = PH_RESV_KW_USING + "(.*?);"
        let regex = try! NSRegularExpression(pattern: pattern, options: [])

        var matches = regex.matches(
            in: tmpCode,
            options: [],
            range: NSRange(location: 0, length: tmpCode.utf16.count)
        )

        while matches.count != 0 {

            for m in matches {
                let matchedText = (tmpCode as NSString).substring(with: m.range)

                let usingName = matchedText
                    .replacingOccurrences(of: PH_RESV_KW_USING, with: "")
                    .replacingOccurrences(of: ";", with: "")

                if !AllUsingNamespaceDeclarations.contains(usingName) {
                    AllUsingNamespaceDeclarations.append(usingName)
                }

                let externFile =
                    currentFilePath
                    + "/"
                    + usingName
                    + ".cs"

                if !FileManager.default.fileExists(atPath: externFile) {
                    code = code.replacingOccurrences(of: matchedText, with: "")
                    continue
                }

                // Prevent loading same files
                if !LoadedFileList.contains(externFile) {
                    var externCode = try! String(contentsOfFile: externFile, encoding: .utf8)
                    // Normalize line breaks to \n
                    externCode = externCode
                        .replacingOccurrences(of: "\r\n", with: "\n")
                        .replacingOccurrences(of: "\r", with: "\n")
                    code = code.replacingOccurrences(of: matchedText, with: "")
                    code = externCode + code
                    LoadedFileList.append(externFile)
                } else {
                    ShowLog("Loading external include file [" + URL(fileURLWithPath: externFile).lastPathComponent + "]")
                    code = code.replacingOccurrences(of: matchedText, with: "")
                }
            }

            // Check code again to see if there's any "using " keyword left over
            CharLiteralList.removeAll()
            StringLiteralList.removeAll()
            ShowLog("Preprocessing with external include file...")

            tmpCode = PreProcessCodeBeforeLOCSeparation(code)
            matches = regex.matches(
                in: tmpCode,
                options: [],
                range: NSRange(location: 0, length: tmpCode.utf16.count)
            )
        }

        CharLiteralList.removeAll()
        StringLiteralList.removeAll()
        return code
    }
    
    /// <summary>
    /// This method generates line of codes that are tokenized,
    /// ready for converting into assembly code
    /// </summary>
    /// <param name="code">The entire program code string</param>
    /// <returns>Cleaned up code lines</returns>
    public static func PreProcessCleanUp(_ input: String) -> [String] {

        var code = input

        // Do some preparations to code by sanitizing unwanted elements
        code = PreProcessCodeBeforeLOCSeparation(code)

        // Convert the program initialization entry point "[DScript_Init]" to a label
        code = code.replacingOccurrences(
            of: PH_RESV_KW_PROGRAM_INIT_POINT_ATTRIBUTE,
            with: PH_RESV_KW_PROGRAM_INIT_POINT_LABEL
        )

        // Convert the program main entry point "[DScript_Main]" to a label
        code = code.replacingOccurrences(
            of: PH_RESV_KW_PROGRAM_MAIN_LOOP_ATTRIBUTE,
            with: PH_RESV_KW_PROGRAM_MAIN_LOOP_LABEL
        )

        // Separate codes by tokens to make code of lines
        var codes = splitCodesToArray(code)

        // Test
        if IsGenerateUnitTestFiles { TestSplitCodesToArray = codes }
        
        ShowLog("- Adding missing scope...");
        // Add missing scope brackets (curly brackets) to "if" "else if" "while" "for"
        codes = AddMissingScopeBrackets(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestAddMissingScopeBrackets = codes }
        
        ShowLog("- Resolving instantiation code...");
        // We need to convert all instantiation code (using the "new" keyword) to call this new constructor
        // So, we will search for all codes that contains the "new" keyword
        codes = ResolveInstantiationCode(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveInstantiationCode = codes }
        
        // Convert all occurance of "string" to "char[]"
        codes = ConvertStringKeywordToCharArray(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertStringKeywordToCharArray = codes }
        
        ShowLog("- Converting pre-defined keyword to actual value...");
        // Replace pre-defined keywords (true, false, null, etc) with actual values (may be numeric, class objects, strings, etc)
        codes = ReplacePreDefinedKeywordsWithActualValue(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestReplacePreDefinedKeywordsWithActualValue = codes }
        
        // Since DScript language does not support float/double internally,
        // if a floating number is found, we need to multiply it by 10^n to move the decimal point rightwards to make it a whole number
        codes = ResolveFloatDoubleSuffix(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveFloatDoubleSuffix = codes }
        
        ShowLog("- Converting arrays to pointers...");
        // Convert arrays (including multi-dimentional arrays) declarations to pointers
        codes = ConvertArraysToCLangPointerNotations(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertArraysToCLangPointerNotations = codes }
        
        ShowLog("- Converting methods to functions...");
        // Convert C# methods to PHP-like functions
        codes = ConvertMethodToFunctions(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertMethodToFunctions = codes }
        
        ShowLog("- Resolving function returns...");
        // Add "return(0);" to the very bottom of functions (ALL functions must return somthing, even if it is a void function)
        codes = AddReturnToFunctions(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestAddReturnToFunctions = codes }
        
        // For each "return(xxx);" statement, separate equation so we only have one variable inside the return bracket: e.g. "var ret = xxx; return(ret);"
        codes = ResolveReturnStatements(codes);
        // Test
        if IsGenerateUnitTestFiles { TestResolveReturnStatements = codes }
        
        
        ShowLog("- Resolving scope...");
        // Adds the specified parent scope definition to its specified child scope definition
        codes = ResolveScope(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveScope = codes }
        
        ShowLog("- Preprocessing pointers...")

        // Get the list of variable types for pointers, and convert them to "ptr𒀭"
        var i = 0
        while i < codes.count {

            defer { i += 1 }   // ← for-loop の i++ を保証

            // if(codes[i].IndexOf(PH_RESV_KW_PTR) == -1) continue;
            if !codes[i].contains(PH_RESV_KW_PTR) {
                continue
            }

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count < 2 {
                continue
            }

            let typeParts = splitCode[0].split(separator: "_")
            if typeParts.isEmpty {
                continue
            }

            let varType = String(typeParts[0])
            let varName = splitCode[1]

            // ActualClassMemberPointerVarTypeList.Add(splitCode[1], varType);
            ActualClassMemberPointerVarTypeList[varName] = varType

            // codes[i] = codes[i].Replace(splitCode[0] + " ", PH_RESV_KW_PTR);
            codes[i] = codes[i].replacingOccurrences(
                of: splitCode[0] + " ",
                with: PH_RESV_KW_PTR
            )
        }
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessPointers = codes }
        
        ShowLog("- Converting labels to placeholders...");
        // Convert labels "labelname:" to placeholder symbols, so we can distinguish between labels and class inheritences
        codes = ConvertLabelsToPlaceHolders(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertLabelsToPlaceHolders = codes }
        
        ShowLog("- Resolving extended classes...");
        // Resolve class inheritences
        codes = ResolveExtendedClass(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveExtendedClass = codes }
        
        
        ShowLog("- Preprocessing static classes...");
        // Adds constructors to static class
        codes = AddConstructorsToStaticClass(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestAddConstructorsToStaticClass = codes }
        
        ShowLog("- Preprocessing for loops...");
        // Preprocess for loop statements by converting them to "while" statements, and adding labels for start and end block of the entire for statement
        codes = PreProcessForBlock(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessForBlock = codes }
        
        ShowLog("- Converting while statements to if statements...");
        // Preprocess while loop statements by convertinf them to "if" statements, and adding labels for start and end block of the entire while statement
        codes = PreProcessWhileBlock(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessWhileBlock = codes }
        
        ShowLog("- Preprocessing if else blocks...");
        // Place the equation for logical comparison inside if/else if statements outside the if/else if brackets, and also place a label for the start and end block of the if statement
        codes = PreProcessIfElseBlock(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessIfElseBlock = codes }
        
        ShowLog("- Resolving floating function calls...");
        // Resolve floating function calls that usually returns some kind of value back to the caller
        codes = ResolveFloatingFunctionCalls(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveFloatingFunctionCalls = codes }
        
        ShowLog("- Preprocessing chars to UTF16...");
        // Convert all char placeholders to actual decimal (UTF-16 / Decimal) representation
        codes = ConvertCharPlaceHolderToDecimal(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertCharPlaceHolderToDecimal = codes }
        
        ShowLog("- Preprocessing expressions...");
        // Preprocess / breakdown expressions
        codes = PreProcessFunctionParams(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessFunctionParams = codes }
        
        codes = PreProcessExpressions(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestPreProcessExpressions = codes }
        
        
        ShowLog("- Resolving implicit var types...");
        // For all "var" keywords, determine their appropriate size if possible - if not able to determine, it will be converted to the most largest variable type (long)
        codes = ResolveImplicitVarType(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveImplicitVarType = codes }
        
        ShowLog("- Restructuring class member vars...");
        // For each class, move all member variable initiation codes to the constructor, and variable assignments to top of class
        codes = RestructureClassMemberVars(codes, PH_RESV_KW_CLASS);
        // For each static class, move all member variable initiation codes to the constructor, and variable assignments to top of class
        codes = RestructureClassMemberVars(codes, PH_RESV_KW_STATIC_CLASS);
        
        // Test
        if IsGenerateUnitTestFiles { TestRestructureClassMemberVars = codes }
        
        ShowLog("- Deploying iasm codes...");
        // Deploy all manually coded IASM codes in the script
        codes = DeployManualIASM(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestDeployManualIASM = codes }
        
        
        ShowLog("- Converting strings to char arrays...");
        // Convert all string placeholders to char arrays assigned with actual decimal (UTF-16 / Decimal) representation
        codes = ConvertStringPlaceHolderToCharDecimalArray(codes);
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertStringPlaceHolderToCharDecimalArray = codes }
        
        ShowLog("- Resolving constructors...");
        // Move any variables that are declared as class members to constructor, if they are not being used in other places other than within the constructor
        codes = MoveTemporaryClassMemberVarToConstructor(codes, PH_RESV_KW_CLASS);
        // Move any variables that are declared as static class members to constructor, if they are not being used in other places other than within the constructor
        codes = MoveTemporaryClassMemberVarToConstructor(codes, PH_RESV_KW_STATIC_CLASS);
        
        if IsGenerateUnitTestFiles { TestMoveTemporaryClassMemberVarToConstructor = codes}
        
        return codes
    }
    
    /// <summary>
    /// Separate codes by tokens to make code of lines,
    /// and eliminates any empty codes
    /// </summary>
    /// <param name="code">The entire string of code</param>
    /// <returns>Separated codes by token</returns>
    public static func splitCodesToArray(_ code: String) -> [String] {

        let regex = try! NSRegularExpression(
            pattern: SeparatorPattern,
            options: [.dotMatchesLineSeparators]
        )

        let nsCode = code as NSString
        let fullRange = NSRange(location: 0, length: nsCode.length)

        var results: [String] = []
        var lastIndex = 0

        // Regex.Split equivalent (including separators)
        regex.enumerateMatches(in: code, options: [], range: fullRange) { match, _, _ in
            guard let match = match else { return }

            // Text before separator
            let rangeBefore = NSRange(
                location: lastIndex,
                length: match.range.location - lastIndex
            )
            if rangeBefore.length > 0 {
                let part = nsCode.substring(with: rangeBefore)
                if part != "" {
                    results.append(part)
                }
            }

            // The separator itself
            let separator = nsCode.substring(with: match.range)
            if separator != "" {
                results.append(separator)
            }

            lastIndex = match.range.location + match.range.length
        }

        // Remaining tail
        if lastIndex < nsCode.length {
            let tailRange = NSRange(
                location: lastIndex,
                length: nsCode.length - lastIndex
            )
            let tail = nsCode.substring(with: tailRange)
            if tail != "" {
                results.append(tail)
            }
        }

        // Get rid of empty / whitespace-only code and trim
        results = results
            .filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }

        return results
    }
    
    /// <summary>
    /// Returns the index where the end scope block appears
    /// </summary>
    public static func FindEndScopeBlock(
        _ codes: [String],
        _ scopeBlockBeginIndex: Int,
        _ openingScopeBlock: String,
        _ endingScopeBlock: String
    ) -> Int {

        var endIndex = -1
        var scopeKeywordCounter = 0

        if openingScopeBlock == "" {
            scopeKeywordCounter = 1
        }

        if scopeBlockBeginIndex < 0 {
            return -1
        }

        var i = scopeBlockBeginIndex
        while i < codes.count {
            if codes[i] == openingScopeBlock {
                scopeKeywordCounter += 1
            }

            if codes[i] == endingScopeBlock {
                scopeKeywordCounter -= 1
                if scopeKeywordCounter == 0 {
                    endIndex = i
                    break
                }
            }

            i += 1
        }

        return endIndex
    }
    
    /// <summary>
    /// Add missing scope brackets (curly brackets) to
    /// "if" "else if" "while" "for"
    /// </summary>
    public static func AddMissingScopeBrackets(
        _ codes: [String]
    ) -> [String] {

        var codeList = codes
        var i = 0

        while i < codeList.count {

            defer { i += 1 }   // ← C# for(i++) 完全再現

            if !RoundBracketScopeHolderKeywords.contains(codeList[i]) {
                continue
            }

            var startRoundBracketIndex = -1

            // Find '('
            var j = i
            while j < codeList.count {
                if codeList[j] == "(" {
                    startRoundBracketIndex = j
                    break
                }
                j += 1
            }

            if startRoundBracketIndex == -1 {
                continue
            }

            let endRoundBracketIndex =
                FindEndScopeBlock(
                    codeList,
                    startRoundBracketIndex,
                    "(",
                    ")"
                )

            if endRoundBracketIndex == -1 {
                continue
            }

            // Already has scope
            if endRoundBracketIndex + 1 < codeList.count,
               codeList[endRoundBracketIndex + 1] == "{" {
                i = endRoundBracketIndex
                continue
            }

            var roundBracketCnt = 0

            // Find first valid ';' outside nested ()
            j = endRoundBracketIndex + 1
            while j < codeList.count {

                defer { j += 1 }   // ← C# の j++ を保証

                if codeList[j] == "(" {
                    roundBracketCnt += 1
                    continue
                }

                if codeList[j] == ")" {
                    roundBracketCnt -= 1
                    continue
                }

                if codeList[j] != ";" {
                    continue
                }

                if roundBracketCnt != 0 {
                    continue
                }

                // Insert scope brackets
                codeList.insert("}", at: j + 1)
                codeList.insert("{", at: endRoundBracketIndex + 1)

                i = endRoundBracketIndex
                break
            }
        }

        return codeList
    }
    
    /// Convert instantiation code using "new" keyword
    public static func ResolveInstantiationCode(
        _ codes: [String]
    ) -> [String] {

        var codeList = codes
        let newKeyword = PH_ID + PH_NEW + PH_ID

        var i = 0
        while i < codeList.count {

            defer { i += 1 }   // ← C# for(i++) を完全再現

            // if(codeList[i].IndexOf(newKeyword) != -1)
            guard codeList[i].contains(newKeyword) else {
                continue
            }

            // Ignore array instantiation
            if codeList[i].contains("[") {
                continue
            }

            // Must be assignment
            guard i - 1 >= 0, codeList[i - 1] == "=" else {
                continue
            }

            guard i - 2 >= 0 else {
                continue
            }

            let classAndInstanceObj =
                codeList[i - 2]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            let className: String
            let instanceObjName: String

            if classAndInstanceObj.count == 1 {
                className =
                    codeList[i].replacingOccurrences(
                        of: newKeyword,
                        with: ""
                    )
                instanceObjName = codeList[i - 2]
            } else {
                className = classAndInstanceObj[0]
                instanceObjName = classAndInstanceObj[1]
            }

            // C# と同じ Insert 順序（i+1 に 4 回）
            codeList.insert("(", at: i + 1)
            codeList.insert(")", at: i + 2)
            codeList.insert(";", at: i + 3)
            codeList.insert(
                instanceObjName + "." + className + "_Constructor",
                at: i + 4
            )
        }

        return codeList
    }
    
    /// <summary>
    /// Convert all occurance of "string" to "char[]"
    /// </summary>
    public static func ConvertStringKeywordToCharArray(
        _ codes: [String]
    ) -> [String] {

        var codeList = codes
        let stringKw = PH_RESV_KW_STRING          // "string "
        let charArrayKw = PH_RESV_KW_CHAR.trimmingCharacters(in: .whitespaces) + "[] "

        var i = 0
        while i < codeList.count {

            let token = codeList[i]

            // C#:
            // if(codeList[i].Length >= PH_RESV_KW_STRING.Length)
            if token.count >= stringKw.count {

                // if(codeList[i].Substring(0, PH_RESV_KW_STRING.Length) == PH_RESV_KW_STRING)
                if token.hasPrefix(stringKw) {

                    // Replace first occurrence
                    codeList[i] = ReplaceFirstOccurance(
                        codeList[i],
                        stringKw,
                        charArrayKw
                    ).trimmingCharacters(in: .whitespaces)

                    // If assignment follows, split declaration and assignment
                    // C#: if(codeList[i + 1] == "=")
                    if i + 1 < codeList.count, codeList[i + 1] == "=" {

                        let parts = codeList[i].split(separator: " ", omittingEmptySubsequences: false)
                        if parts.count >= 2 {
                            let strVariableName = String(parts[1])

                            // Replace "=" with ";"
                            codeList[i + 1] = ";"

                            // Insert: varName =
                            codeList.insert(strVariableName, at: i + 2)
                            codeList.insert("=", at: i + 3)

                            // Advance index to skip inserted tokens
                            i += 2
                        }
                    }
                }
            }

            i += 1
        }

        return codeList
    }
    
    /// Replace pre-defined keywords (true, false, null, etc)
    /// with actual numeric values
    public static func ReplacePreDefinedKeywordsWithActualValue(
        _ codes: [String]
    ) -> [String] {

        var codes = codes

        var i = 0
        while i < codes.count {

            switch codes[i] {
            case PH_RESV_KW_FALSE:
                codes[i] = "0"

            case PH_RESV_KW_TRUE:
                codes[i] = "1"

            case PH_RESV_KW_NULL:
                codes[i] = "0"

            default:
                break
            }

            i += 1
        }

        return codes
    }
    
    /// Since DScript language does not support float/double internally,
    /// convert floating literals with suffix (f/F/d/D)
    /// into scaled integers.
    public static func ResolveFloatDoubleSuffix(
        _ codes: [String]
    ) -> [String] {

        var codes = codes
        let suffixes: [Character] = ["f", "F", "d", "D"]

        var i = 0
        while i < codes.count {

            let token = codes[i]

            // Empty guard
            if token.isEmpty {
                i += 1
                continue
            }

            // 1. Check suffix at last character
            guard let lastChar = token.last,
                  suffixes.contains(lastChar)
            else {
                i += 1
                continue
            }

            // 2. Remove suffix
            let numericPart = String(token.dropLast())

            // 3. Numeric check (digits + optional dot)
            let numberCheck = numericPart.replacingOccurrences(of: ".", with: "")
            if numberCheck.isEmpty ||
               numberCheck.range(of: #"^[0-9]+$"#, options: .regularExpression) == nil {
                i += 1
                continue
            }

            // 4. Convert to Double
            guard let value = Double(numericPart) else {
                i += 1
                continue
            }

            // 5. Normalize
            let normalizedValue =
                Int64(value * DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY)

            // 6. Cast depending on architecture
            switch TARGET_ARCH_BIT_SIZE {
            case 16:
                codes[i] = String(Int16(truncatingIfNeeded: normalizedValue))
            case 32:
                codes[i] = String(Int32(truncatingIfNeeded: normalizedValue))
            case 64:
                codes[i] = String(Int64(truncatingIfNeeded: normalizedValue))
            default:
                break
            }

            i += 1
        }

        return codes
    }
    
    /// Convert arrays (including multi-dimentional arrays) declarations to pointers
    public static func ConvertArraysToCLangPointerNotations(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var i = 0

        while i < codes.count {

            let token = codes[i]

            // --------------------------------------------------
            // ① One-dimensional array: "int[] a"
            // --------------------------------------------------
            if token.contains("[]"), token.contains(" ") {

                let splitCode = token.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                if splitCode.count >= 2 {

                    let rawType = splitCode[0].replacingOccurrences(of: "[]", with: "")
                    _ = splitCode[1]

                    // char[] → char_ptr , int[] → int_ptr
                    var varType = rawType + "_" + PH_RESV_KW_PTR

                    // Only primitive types get typed pointers
                    if !AllPrimitiveTypePointerKeywords.contains(varType) {
                        varType = PH_RESV_KW_PTR
                    }

                    // Regex: (.*?)\]
                    let regex = try! NSRegularExpression(pattern: #"(.*?)\] "#)
                    let range = NSRange(location: 0, length: token.utf16.count)

                    codes[i] = regex.stringByReplacingMatches(
                        in: token,
                        options: [],
                        range: range,
                        withTemplate: varType
                    )
                }

            }
            // --------------------------------------------------
            // ② Multi-dimensional array start: "string["
            // --------------------------------------------------
            else if token.hasSuffix("[") {

                var endIndex = -1
                var j = i + 1

                while j < codes.count {

                    if codes[j].contains("] ") {

                        endIndex = j

                        // Get variable name from "] name"
                        let splitCode = codes[j].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                        if splitCode.count >= 2 {
                            let varName = splitCode[1]

                            // Replace "string[" → "ptr name"
                            let regex = try! NSRegularExpression(pattern: #"(.*?)\["#)
                            let range = NSRange(location: 0, length: codes[i].utf16.count)

                            codes[i] = regex.stringByReplacingMatches(
                                in: codes[i],
                                options: [],
                                range: range,
                                withTemplate: PH_RESV_KW_PTR + varName
                            )
                        }

                        // Remove closing token
                        codes[j] = ""
                        i = endIndex
                        break

                    } else {
                        // Remove intermediate ","
                        codes[j] = ""
                    }

                    j += 1
                }
            }

            i += 1
        }

        // Remove empty tokens
        codes = codes
            .filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }

        return codes
    }
    
    /// <summary>
    /// Stores the indexes of the keyword in the code string array
    /// </summary>
    /// <param name="codes">The string array of tokenized code</param>
    /// <param name="keyword">Scope keyword definition (typically with a trailing space)</param>
    /// <returns></returns>
    public static func GenerateKeywordIndexList(
        _ codes: [String],
        _ keyword: String
    ) -> [Int] {

        var result: [Int] = []

        for i in 0..<codes.count {
            let token = codes[i]

            // C#:
            // if(codes[i].IndexOf(keyword) == -1) continue;
            guard token.contains(keyword) else { continue }

            // if(codes[i].IndexOf(keyword, 0, keyword.Length) != -1)
            if token.hasPrefix(keyword) {
                result.append(i)
            }
        }

        return result
    }
    
    /// <summary>
    /// Get the list of class names
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func GetAllKeywordDeclarations(
        _ codes: [String],
        _ keyword: String,
        _ isObtainParentClass: Bool = false
    ) -> [String] {

        var classNameList: [String] = []

        let indexes = GenerateKeywordIndexList(codes, keyword)

        for idx in indexes {

            // C#: codes[idx].Substring(keyword.Length)
            let namePart = String(codes[idx].dropFirst(keyword.count))

            if !classNameList.contains(namePart) {
                var fullName = namePart

                if isObtainParentClass {
                    if idx + 2 < codes.count, codes[idx + 1] == ":" {
                        fullName += ":" + codes[idx + 2]
                    }
                }

                classNameList.append(fullName)
            }
        }

        return classNameList
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
    public static func ConvertMethodToFunctions(
        _ codes: [String]
    ) -> [String] {

        var codes = codes

        // Get all class names
        let classList = GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false)

        var i = 0
        while i < codes.count {

            if codes[i] != ")" {
                i += 1
                continue
            }

            // Method must be followed by "{"
            if i + 1 < codes.count, codes[i + 1] == "{" {

                var bracketCnt = 1
                var j = i - 1

                // Trace back to find matching "("
                while j >= 0 {

                    if codes[j] == ")" {
                        bracketCnt += 1
                    } else if codes[j] == "(" {
                        bracketCnt -= 1
                    }

                    if codes[j] == "(" && bracketCnt == 0 {

                        // Ignore non-method keywords
                        if NonMethodKeywords.contains(codes[j - 1]) {
                            break
                        }

                        let split = codes[j - 1].split(separator: " ", omittingEmptySubsequences: false).map(String.init)

                        var funcReturnType: String

                        if split.count == 1 {
                            // Constructor
                            funcReturnType = PH_RESV_KW_FUNCTION_VOID.trimmingCharacters(in: .whitespaces)
                        } else if classList.contains(split[0]) {
                            funcReturnType = PH_RESV_KW_FUNCTION_PTR.trimmingCharacters(in: .whitespaces)
                        } else if split[0].contains(PH_RESV_KW_PTR.trimmingCharacters(in: .whitespaces)) {
                            funcReturnType = PH_RESV_KW_FUNCTION_PTR.trimmingCharacters(in: .whitespaces)
                        } else {
                            funcReturnType = PH_RESV_KW_FUNCTION.trimmingCharacters(in: .whitespaces)
                                + "_"
                                + split[0]
                        }

                        // Final rewrite
                        codes[j - 1] = funcReturnType + " " + split.last!

                        break
                    }

                    j -= 1
                }
            }

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Add "return(0);" to the very bottom of functions (ALL functions must return somthing, even if it is a void function)
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func AddReturnToFunctions(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        var i = 0
        while i < codes.count {

            let funcSplit = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if funcSplit.isEmpty {
                i += 1
                continue
            }

            // C#: if(!AllFunctionKeywords.Contains(funcSplit[0] + " ")) continue;
            let keyword = funcSplit[0] + " "
            if !AllFunctionKeywords.contains(keyword) {
                i += 1
                continue
            }

            // Find function scope
            let endRoundBracketScopeIndex =
                FindEndScopeBlock(codes, i, "(", ")")

            if endRoundBracketScopeIndex == -1 {
                i += 1
                continue
            }

            let startScopeBlockIndex = endRoundBracketScopeIndex + 1
            let endScopeBlockIndex =
                FindEndScopeBlock(codes, startScopeBlockIndex, "{", "}")

            if endScopeBlockIndex == -1 {
                i += 1
                continue
            }

            // Only void functions
            if keyword == PH_RESV_KW_FUNCTION_VOID {

                // Add final "return(0);}" before closing brace
                codes[endScopeBlockIndex] =
                    PH_RESV_KW_RETURN + "(0);" + "}"

                // Convert all inner "return;" → "return(0);"
                var j = startScopeBlockIndex
                while j < endScopeBlockIndex {
                    if codes[j] + " " == PH_RESV_KW_RETURN {
                        if j + 1 < codes.count {
                            codes[j + 1] = "(0);"
                        }
                    }
                    j += 1
                }
            }

            i += 1
        }

        // Re-tokenize (same as C#)
        codes = RegenerateCleanCode(codes)
        
        return codes
    }
    
    /// Clean the codes by separating into tokens again
    public static func RegenerateCleanCode(
        _ codes: [String]
    ) -> [String] {

        var newCodes = ""
        for c in codes {
            newCodes += c
        }
        return splitCodesToArray(newCodes)
    }
    
    /// <summary>
    /// For each "return(xxx);" statement, separate equation so we only have one variable inside the return bracket: e.g. "var ret = xxx; return(ret);"
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ResolveReturnStatements(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        var i = 0
        while i < codes.count {

            let funcSplit = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if funcSplit.isEmpty {
                i += 1
                continue
            }

            // C#: if(!AllFunctionKeywords.Contains(funcSplit[0] + " ")) continue;
            let keyword = funcSplit[0] + " "
            if !AllFunctionKeywords.contains(keyword) {
                i += 1
                continue
            }

            // function_xxx → get return type
            let funcNameSplit = funcSplit[0].split(separator: "_").map(String.init)
            var funcType = funcNameSplit.count > 1 ? funcNameSplit[1] : ""

            let endRoundBracketScopeIndex =
                FindEndScopeBlock(codes, i, "(", ")")
            if endRoundBracketScopeIndex == -1 {
                i += 1
                continue
            }

            let startScopeBlockIndex = endRoundBracketScopeIndex + 1
            let endScopeBlockIndex =
                FindEndScopeBlock(codes, startScopeBlockIndex, "{", "}")
            if endScopeBlockIndex == -1 {
                i += 1
                continue
            }

            // void → byte
            if funcType + " " == PH_RESV_KW_VOID {
                funcType = PH_RESV_KW_BYTE
            }

            var j = startScopeBlockIndex
            while j < endScopeBlockIndex {

                if codes[j] + " " == PH_RESV_KW_RETURN {

                    // Keep "return(0);" as-is
                    if j + 3 < codes.count,
                       codes[j + 2] == "0",
                       codes[j + 3] == ")" {
                        j += 1
                        continue
                    }

                    let endReturnBlockIndex =
                        FindEndScopeBlock(codes, j + 1, "(", ")")
                    if endReturnBlockIndex == -1 {
                        j += 1
                        continue
                    }

                    // Create temp variable
                    let tmpVar = RandomString(
                        RND_ALPHABET_STRING_LEN_MAX,
                        true,
                        TmpVarIdentifier
                    )

                    var expression = funcType + " " + tmpVar + "="
                    if keyword == PH_RESV_KW_FUNCTION_PTR {
                        expression = PH_RESV_KW_PTR + tmpVar + "="
                    }

                    // Copy expression inside return(...)
                    var k = j + 2
                    while k < endReturnBlockIndex {
                        expression += codes[k]
                        codes[k] = ""
                        k += 1
                    }

                    // Rewrite return
                    codes[j + 2] = tmpVar
                    codes[j] = expression + ";" + PH_RESV_KW_RETURN

                    j = endReturnBlockIndex
                }

                j += 1
            }

            i += 1
        }

        // Re-tokenize
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Adds the specified parent scope definition to its specified child scope definition
    /// e.g. If parent scope def "namespace " (e.g. "namespace A"), and child scope def is "class ", then "class C" becomes: "class A.C"
    /// This method will iterate through the whole code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ResolveScope(_ codes: [String]) -> [String] {

        var codes = codes

        ShowLog(" - Resolving nested namespaces...")
        // Normalize namespace's nested namespaces
        for index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE) {
            codes = AddKeywordScope(
                codes,
                index,
                PH_RESV_KW_NAMESPACE,
                PH_RESV_KW_NAMESPACE,
                "{",
                "}",
                0
            )
        }

        ShowLog(" - Resolving namespace classes...")
        // Normalize namespace's classes
        for index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE) {
            codes = AddKeywordScope(
                codes,
                index,
                PH_RESV_KW_NAMESPACE,
                PH_RESV_KW_CLASS,
                "{",
                "}",
                0
            )
        }

        ShowLog(" - Resolving namespace static classes...")
        // Normalize namespace's static classes
        for index in GenerateKeywordIndexList(codes, PH_RESV_KW_NAMESPACE) {
            codes = AddKeywordScope(
                codes,
                index,
                PH_RESV_KW_NAMESPACE,
                PH_RESV_KW_STATIC_CLASS,
                "{",
                "}",
                0
            )
        }
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase1 = codes }
        
        // Join the reserved keyword list with the class names
        let mergedKeywordList = GenerateDefinedTypeList(codes)
        
        ShowLog(" - Adding namespaces for class variable definitions...")
        // Add namespaces for class variable definition (classes)
        codes = ResolveClassTypeDefinitionScopes(codes, false)

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase2 = codes }

        
        ShowLog(" - Resolving function parameters...")
        // Normalize function parameters
        var keywordIndexListFunc: [[Int]] = Array(
            repeating: [],
            count: AllFunctionKeywords.count
        )

        let funcKeyword = PH_RESV_KW_FUNCTION.trimmingCharacters(in: .whitespaces) + "_"

        for i in 0..<codes.count {
            if !codes[i].contains(funcKeyword) { continue }
            for j in 0..<AllFunctionKeywords.count {
                let kw = AllFunctionKeywords[j]
                if codes[i].hasPrefix(kw) {
                    keywordIndexListFunc[j].append(i)
                    break
                }
            }
        }

        for s in mergedKeywordList {
            for i in 0..<AllFunctionKeywords.count {
                for index in keywordIndexListFunc[i] {
                    codes = AddKeywordScope(
                        codes,
                        index,
                        AllFunctionKeywords[i],
                        s,
                        "(",
                        ")"
                    )
                }
            }
        }

        ShowLog(" - Resolving static classes...")
        // Temporarily convert static classes to normal classes
        var staticClassDefList: [Int] = []

        for i in 0..<codes.count {
            if codes[i].hasPrefix(PH_RESV_KW_STATIC_CLASS) {
                codes[i] = codes[i].replacingOccurrences(
                    of: PH_RESV_KW_STATIC_CLASS,
                    with: PH_RESV_KW_CLASS
                )
                staticClassDefList.append(i)
            }
        }

        ShowLog(" - Resolving class properties and functions...")
        // Normalize class members
        for s in mergedKeywordList {
            for index in GenerateKeywordIndexList(codes, PH_RESV_KW_CLASS) {
                codes = AddKeywordScope(
                    codes,
                    index,
                    PH_RESV_KW_CLASS,
                    s,
                    "{",
                    "}",
                    0
                )
            }
        }

        ShowLog(" - Resolving function scope codes...")
        // Normalize everything inside a function
        for s in mergedKeywordList {
            for i in 0..<AllFunctionKeywords.count {
                for index in keywordIndexListFunc[i] {
                    codes = AddKeywordScope(
                        codes,
                        index,
                        AllFunctionKeywords[i],
                        s,
                        "{",
                        "}"
                    )
                }
            }
        }

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase3 = codes; }
        
        
        ShowLog(" - Resolving variables inside class/function scope...")
        // Apply normalized variable scope
        for kw in AllFunctionKeywords {
            codes = ApplyNormalizedVarToScope(codes, kw)
        }
        codes = ApplyNormalizedVarToScope(codes, PH_RESV_KW_CLASS)

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase4 = codes; }
        
        ShowLog(" - Reverting back temporary classes to static classes...")
        // Revert static classes
        for index in staticClassDefList {
            codes[index] = codes[index].replacingOccurrences(
                of: PH_RESV_KW_CLASS,
                with: PH_RESV_KW_STATIC_CLASS
            )
        }

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase5 = codes; }
        
        ShowLog(" - Adding namespaces for floating classes...")
        codes = ResolveClassTypeDefinitionScopes(codes, true)

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase6 = codes; }
        
        ShowLog(" - Adding namespaces and class for function calls...")
        codes = ResolveFunctionCallScopes(codes)

        // Test
        if IsGenerateUnitTestFiles { TestResolveScope_Phase7 = codes; }
        
        ShowLog(" - Resolving \"base\" keyword scope...")
        codes = ResolveBaseKeywordScope(codes)
        
        return codes
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
    public static func AddKeywordScope(
        _ inputCodes: [String],
        _ startCodeArrayIndex: Int,
        _ parentScopeKeyword: String,
        _ childScopeKeyword: String,
        _ openingScopeBlock: String,
        _ endingScopeBlock: String,
        _ limitToScopeHierarchyLevel: Int = -1
    ) -> [String] {

        var codes = inputCodes

        // Search for keyword namespace
        var scopeBlockBeginIndex = -1

        let parentScopeDefName =
            ReplaceFirstOccurance(
                codes[startCodeArrayIndex],
                parentScopeKeyword,
                ""
            )

        codes[startCodeArrayIndex] =
            parentScopeKeyword + parentScopeDefName

        // Get the first index where the scope block begins
        var i = startCodeArrayIndex + 1
        while i < codes.count {
            if codes[i] == openingScopeBlock {
                scopeBlockBeginIndex = i
                break
            }
            i += 1
        }

        if scopeBlockBeginIndex == -1 {
            return codes
        }

        let scopeBlockEndIndex =
            FindEndScopeBlock(
                codes,
                scopeBlockBeginIndex,
                openingScopeBlock,
                endingScopeBlock
            )

        if scopeBlockEndIndex == -1 {
            return codes
        }

        let replaceWith =
            childScopeKeyword + parentScopeDefName + "."

        codes =
            ReplaceScopeKeywordsWith(
                codes,
                scopeBlockBeginIndex,
                scopeBlockEndIndex,
                openingScopeBlock,
                endingScopeBlock,
                childScopeKeyword,
                replaceWith,
                limitToScopeHierarchyLevel
            )

        return codes
    }
    
    public static func ReplaceScopeKeywordsWith(
        _ inputCodes: [String],
        _ scopeBlockBeginIndex: Int,
        _ scopeBlockEndIndex: Int,
        _ openingScopeBlock: String,
        _ endingScopeBlock: String,
        _ childScopeKeyword: String,
        _ replaceWith: String,
        _ limitToScopeHierarchyLevel: Int,
        _ isOnlyReplaceMethods: Bool = false
    ) -> [String] {

        var codes = inputCodes
        var scopeHierarchyLevel = -1

        var i = scopeBlockBeginIndex
        while i < scopeBlockEndIndex {

            if codes[i] == openingScopeBlock {
                scopeHierarchyLevel += 1
            }
            if codes[i] == endingScopeBlock {
                scopeHierarchyLevel -= 1
            }

            if limitToScopeHierarchyLevel != -1 {
                if scopeHierarchyLevel != limitToScopeHierarchyLevel {
                    i += 1
                    continue
                }
            }

            if !codes[i].contains(childScopeKeyword) {
                i += 1
                continue
            }

            if isOnlyReplaceMethods {
                // Methods must be followed by "("
                if i + 1 >= codes.count || codes[i + 1] != "(" {
                    i += 1
                    continue
                }
            }

            if codes[i].hasPrefix(childScopeKeyword) {
                codes[i] =
                    ReplaceFirstOccurance(
                        codes[i],
                        childScopeKeyword,
                        replaceWith
                    )
            }

            i += 1
        }

        return codes
    }
    
    /// Join the reserved keyword list with the class names
    public static func GenerateDefinedTypeList(
        _ codes: [String]
    ) -> [String] {

        // To normalize class objects, first, get all the class names
        let classNameList =
            GetAllKeywordDeclarations(
                codes,
                PH_RESV_KW_CLASS,
                false
            )

        // Merge all keywords together
        var keywordList: [String] = []

        // Add class member reserved keywords
        for s in ClassMemberReservedKeyWords {
            keywordList.append(s)
        }

        // Add class names as keywords
        // NOTE: add trailing space to match C# behavior
        for s in classNameList {
            keywordList.append(s + " ")
        }

        return keywordList
    }
    
    /// Add namespaces for class type definitions
    /// e.g.
    /// CLASSNAME c = new CLASSNAME();
    /// -->
    /// NS.CLASSNAME c = new CLASSNAME();
    public static func ResolveClassTypeDefinitionScopes(
        _ inputCodes: [String],
        _ isResolveFloatingObject: Bool = false
    ) -> [String] {

        var codes = inputCodes

        // Get all names declared using the "using" keyword
        let usingNameList = AllUsingNamespaceDeclarations

        // Get list of class names
        var classDeclarationList =
            GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false)

        // Get list of static class names
        let staticClassDeclarationList =
            GetAllKeywordDeclarations(codes, PH_RESV_KW_STATIC_CLASS, false)

        // Merge normal + static classes
        classDeclarationList.append(contentsOf: staticClassDeclarationList)

        var nameSpaceList: [String] = []

        var i = 0
        while i < codes.count {

            // Track namespace stack
            if codes[i].hasPrefix(PH_RESV_KW_NAMESPACE) {
                let ns =
                    String(codes[i].dropFirst(PH_RESV_KW_NAMESPACE.count))
                nameSpaceList.append(ns)
            }

            let split = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            var currentClassName = ""

            // Handle new keyword
            if codes[i].contains(PH_ID + PH_NEW + PH_ID) {
                currentClassName =
                    codes[i].replacingOccurrences(
                        of: PH_ID + PH_NEW + PH_ID,
                        with: ""
                    )
            } else {
                if !isResolveFloatingObject && split.count < 2 {
                    i += 1
                    continue
                }
                currentClassName = split.first ?? ""
            }

            // Already has namespace?
            if currentClassName.contains(".") {
                let parts = currentClassName.split(separator: ".").map(String.init)
                if usingNameList.contains(parts.first ?? "") {
                    i += 1
                    continue
                }
                // Strip last segment
                currentClassName = parts.dropLast().joined(separator: ".")
            }

            // Regex check: valid identifier
            let validIdPattern = "[a-zA-Z_\(PH_ID)]"
            if currentClassName.range(of: validIdPattern, options: .regularExpression) == nil {
                i += 1
                continue
            }

            // Ignore reserved keywords
            if AllReservedKeywords.contains(currentClassName + " ") {
                i += 1
                continue
            }

            var isScopeFound = false

            // Try current namespace stack
            for ns in nameSpaceList {
                let checkClassScope = ns + "." + currentClassName
                let stripped =
                    checkClassScope.replacingOccurrences(
                        of: #"\[(.*?)\]"#,
                        with: "",
                        options: .regularExpression
                    )

                if classDeclarationList.contains(stripped) {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            currentClassName,
                            checkClassScope
                        )
                    isScopeFound = true
                    break
                }
            }

            if isScopeFound {
                i += 1
                continue
            }

            // Search using namespaces
            var usingNSClassDeclarationList = classDeclarationList

            // Filter by using list
            for idx in 0..<usingNSClassDeclarationList.count {
                let parts = usingNSClassDeclarationList[idx].split(separator: ".").map(String.init)
                let ns =
                    parts.dropLast().joined(separator: ".")
                let check = ns.isEmpty ? parts.first ?? "" : ns
                if !usingNameList.contains(check) {
                    usingNSClassDeclarationList[idx] = ""
                }
            }

            // Try match
            for fullName in usingNSClassDeclarationList where !fullName.isEmpty {
                let parts = fullName.split(separator: ".").map(String.init)
                if parts.last == currentClassName {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            currentClassName,
                            fullName
                        )
                    break
                }
            }

            i += 1
        }

        return codes
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
    public static func ApplyNormalizedVarToScope(
        _ inputCodes: [String],
        _ reservedKeyword: String
    ) -> [String] {

        var codes = inputCodes

        for scopeBlockBeginIndex in GenerateKeywordIndexList(codes, reservedKeyword) {

            let scopeBlockEndIndex =
                FindEndScopeBlock(
                    codes,
                    scopeBlockBeginIndex + 1,
                    "{",
                    "}"
                )

            if scopeBlockEndIndex == -1 { continue }

            // -------------------------------------------------
            // Resolve "this" keyword (class scope only)
            // -------------------------------------------------
            if reservedKeyword == PH_RESV_KW_CLASS {

                let className =
                    ReplaceFirstOccurance(
                        codes[scopeBlockBeginIndex],
                        PH_RESV_KW_CLASS,
                        ""
                    )

                for i in scopeBlockBeginIndex..<scopeBlockEndIndex {
                    if codes[i].contains(PH_RESV_KW_THIS) {
                        let pattern = PH_RESV_KW_THIS + #"[^a-zA-Z0-9_]"#
                        if codes[i] == PH_RESV_KW_THIS ||
                            codes[i].range(of: pattern, options: .regularExpression) != nil {

                            codes[i] =
                                ReplaceFirstOccurance(
                                    codes[i],
                                    PH_RESV_KW_THIS,
                                    className
                                )
                        }
                    }
                }
            }

            // -------------------------------------------------
            // Generate declared variable list
            // -------------------------------------------------
            let declaredVars =
                GenerateDeclaredVarList(
                    codes,
                    scopeBlockBeginIndex,
                    scopeBlockEndIndex,
                    reservedKeyword == PH_RESV_KW_CLASS
                )

            // -------------------------------------------------
            // Replace variable references inside scope
            // -------------------------------------------------
            for (fullVarName, _) in declaredVars {

                let originalParts = fullVarName.split(separator: ".").map(String.init)
                let originalName = originalParts.last!

                for i in scopeBlockBeginIndex..<scopeBlockEndIndex {

                    let token = codes[i]

                    // -----------------------------------------
                    // Case 1: no array access
                    // -----------------------------------------
                    if !token.contains("[") {

                        // Exact match
                        if token == originalName {
                            codes[i] = fullVarName
                            continue
                        }

                        // Member access: obj.xxx
                        let parts = token.split(separator: ".").map(String.init)
                        if parts.first == originalName {
                            codes[i] =
                                ReplaceFirstOccurance(
                                    token,
                                    originalName,
                                    fullVarName
                                )
                            continue
                        }
                    }
                    // -----------------------------------------
                    // Case 2: array access
                    // -----------------------------------------
                    else {
                        // C# equivalent: Regex.Split(token, @"([\[\]])")
                        let regex = try! NSRegularExpression(
                            pattern: #"([\[\]])"#,
                            options: []
                        )

                        let nsToken = token as NSString
                        let fullRange = NSRange(location: 0, length: nsToken.length)

                        var parts: [String] = []
                        var lastIndex = 0

                        regex.enumerateMatches(in: token, options: [], range: fullRange) { match, _, _ in
                            guard let match = match else { return }

                            let rangeBefore = NSRange(
                                location: lastIndex,
                                length: match.range.location - lastIndex
                            )

                            if rangeBefore.length > 0 {
                                parts.append(nsToken.substring(with: rangeBefore))
                            }

                            // Keep "[" or "]" tokens
                            parts.append(nsToken.substring(with: match.range))
                            lastIndex = match.range.location + match.range.length
                        }

                        if lastIndex < nsToken.length {
                            let tail = nsToken.substring(from: lastIndex)
                            if !tail.isEmpty {
                                parts.append(tail)
                            }
                        }

                        let checkVarSplit = parts
                            .map { $0.trimmingCharacters(in: .whitespaces) }
                            .filter { !$0.isEmpty }

                        var replaced = false
                        var newParts: [String] = []

                        for part in checkVarSplit {
                            let dotSplit = part.split(separator: ".").map(String.init)
                            if dotSplit.first == originalName {
                                newParts.append(
                                    ReplaceFirstOccurance(
                                        part,
                                        originalName,
                                        fullVarName
                                    )
                                )
                                replaced = true
                            } else {
                                newParts.append(part)
                            }
                        }

                        if replaced {
                            codes[i] = newParts.joined()
                        }
                    }
                }
            }
        }

        return codes
    }
    
    /// Generate variable declarations inside a scope
    public static func GenerateDeclaredVarList(
        _ codes: [String],
        _ scopeBlockBeginIndex: Int,
        _ scopeBlockEndIndex: Int,
        _ ignoreFunctionScopes: Bool = false
    ) -> [String: Int] {

        var varList: [String: Int] = [:]

        var i = scopeBlockBeginIndex
        while i < scopeBlockEndIndex {

            // If requested, skip over function bodies entirely (class-scope variable collection should ignore function-local vars)
            if ignoreFunctionScopes {
                let headSplit = codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false).map(String.init)

                if !headSplit.isEmpty {
                    let kw = headSplit[0] + " "
                    if AllFunctionKeywords.contains(kw) {
                        let endFunc = FindEndScopeBlock(codes, i, "{", "}")
                        if endFunc != -1 {
                            i = endFunc + 1
                            continue
                        }
                    }
                }
            }

            let split = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if split.count < 2 {
                i += 1
                continue
            }

            let typeName = split[0]
            let varName = split[1]

            // Ignore non-variable keywords
            if NonMethodKeywords.contains(typeName) {
                i += 1
                continue
            }

            if !NonVariableDeclareKeywords.contains(typeName + " ") {
                if varList[varName] == nil {
                    varList[varName] = i
                }
            }

            i += 1
        }

        return varList
    }
    
    /// Add namespaces & class for function calls
    public static func ResolveFunctionCallScopes(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        // --- Collect all function declarations ---
        var funcList: [String] = []

        for kw in AllFunctionKeywords {
            let functions = GetAllKeywordDeclarations(codes, kw, false)
            funcList.append(contentsOf: functions)
        }

        let functionDeclarationList = funcList

        // --- Walk through all tokens ---
        for i in 0..<codes.count {

            // Ignore tokens with space (not a simple identifier)
            if codes[i].contains(" ") { continue }

            // Ignore reserved keywords
            if AllReservedKeywords.contains(codes[i] + " ") { continue }

            let currentFunctionName = codes[i]

            // Already scoped
            if currentFunctionName.contains(".") { continue }

            // Placeholder or invalid identifier
            if currentFunctionName.contains(PH_ID) { continue }
            if currentFunctionName.range(of: #"[a-zA-Z_]"#, options: .regularExpression) == nil {
                continue
            }

            // --------------------------------------------------
            // ① Resolve by current class / static class
            // --------------------------------------------------
            var scopeClassName = ""

            var j = i
            while j >= 0 {
                if codes[j].hasPrefix(PH_RESV_KW_STATIC_CLASS) {
                    scopeClassName =
                        codes[j].replacingOccurrences(
                            of: PH_RESV_KW_STATIC_CLASS,
                            with: ""
                        )
                    break
                } else if codes[j].hasPrefix(PH_RESV_KW_CLASS) {
                    scopeClassName =
                        codes[j].replacingOccurrences(
                            of: PH_RESV_KW_CLASS,
                            with: ""
                        )
                    break
                }
                j -= 1
            }

            if !scopeClassName.isEmpty {
                let resolved = scopeClassName + "." + currentFunctionName
                if functionDeclarationList.contains(resolved) {
                    codes[i] = resolved
                    continue
                }
            }

            // --------------------------------------------------
            // ② Resolve via base class
            // --------------------------------------------------
            var baseClassName = ""

            j = i
            while j >= 0 {

                if codes[j] == ":" {
                    baseClassName = codes[j + 1]

                    var isFunctionFound = false

                    for fn in functionDeclarationList {
                        if fn == baseClassName + "." + currentFunctionName {
                            codes[i] = scopeClassName + "." + currentFunctionName
                            isFunctionFound = true
                            break
                        }
                    }

                    if isFunctionFound { break }

                    // Traverse deeper base classes
                    for k in 0..<codes.count {
                        if codes[k] == ":" {
                            if codes[k - 1] == PH_RESV_KW_CLASS + baseClassName {
                                j = k + 1
                                break
                            }
                        }
                    }
                }

                j -= 1
            }
        }

        return codes
    }
    
    /// <summary>
    /// Resolve "base" keyword scope
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ResolveBaseKeywordScope(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        for i in 0..<codes.count {

            // Contains "base" at all?
            if !codes[i].contains(PH_RESV_KW_BASE) {
                continue
            }

            // Regex: base followed by non-identifier char
            let pattern = PH_RESV_KW_BASE + #"[^a-zA-Z0-9_]"#
            if codes[i].range(of: pattern, options: .regularExpression) == nil {
                continue
            }

            // Traverse backwards to find owning class
            var j = i
            while j >= 0 {

                if codes[j].hasPrefix(PH_RESV_KW_CLASS) {

                    let className =
                        codes[j].replacingOccurrences(
                            of: PH_RESV_KW_CLASS,
                            with: ""
                        )

                    // C# assumes: class Child : Parent
                    // → Parent is at j + 2
                    if j + 2 < codes.count {
                        let parentClassName = codes[j + 2]

                        codes[i] =
                            ReplaceFirstOccurance(
                                codes[i],
                                PH_RESV_KW_BASE,
                                className + "." + parentClassName
                            )
                    }

                    break
                }

                j -= 1
            }
        }

        return codes
    }
    
    /// <summary>
    /// Convert labels "labelname:" to placeholder symbols, so we can distinguish between labels and class inheritences
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ConvertLabelsToPlaceHolders(
        _ codes: [String]
    ) -> [String] {

        var codes = codes

        for i in 0..<codes.count {

            // if(codes[i] != ":") continue;
            if codes[i] != ":" {
                continue
            }

            // Ignore if the colon does not belong to a class inheritance
            // if(codes[i - 1].IndexOf(PH_RESV_KW_CLASS, 0) != -1) continue;
            if i - 1 >= 0,
               codes[i - 1].contains(PH_RESV_KW_CLASS) {
                continue
            }

            // label found
            // codes[i - 1] = PH_ID + PH_LABEL + PH_ID + codes[i - 1].Replace(":", "");
            if i - 1 >= 0 {
                codes[i - 1] =
                    PH_ID
                    + PH_LABEL
                    + PH_ID
                    + codes[i - 1].replacingOccurrences(of: ":", with: "")
            }

            // codes[i] = ";";
            codes[i] = ";"
        }

        return codes
    }
    
    /// <summary>
    /// Merges extended parent class to the current class
    /// NOTE: The Parent class remains unmodified as it might be used as a stand alone class elsewhere
    /// e.g.
    /// class ParentTest {
    ///        int a;
    ///        int b;
    ///        function A {}
    ///        function B {}
    /// }
    /// class Test : ParentTest {
    ///        int a;
    ///        int c;
    ///        function A {}
    /// }
    ///
    /// -------> MERGE/CONVERT TO ------>
    ///
    /// class Test {
    ///        int ParentTest_a;  // If duplicates found, parent class name is added before the variable/function, and are treated as separate entities
    ///        int a;
    ///        int b;
    ///        int c;  // NOTE: If a variable or function only exists in the parent class, it can be accessed by either "c" or "base.c" or "this.c" by the derived class
    ///
    ///        function ParentTest_A {}   // If duplicates found, parent class name is added before the variable/function, and are treated as separate entities
    ///        function A {}
    ///        function B {}  // NOTE: If a variable or function only exists in the parent class, it can be accessed by either "B()" or "base.B()" or "this.B()" by the derived class
    /// }
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ResolveExtendedClass(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        // Generates a list of classes, reordered in dependency order
        let reorderedClassList =
            GenerateClassListReorderedByDependancy(codes, false)

        // Test
        //TestGenerateClassListReorderedByDependancy = reorderedClassList
        
        // For each class, resolve extended classes
        for className in reorderedClassList {

            for j in 0..<codes.count {

                // if(codes[j] == PH_RESV_KW_CLASS + reorderedClassList[i])
                if codes[j] == PH_RESV_KW_CLASS + className {
                    // Merge parent → derived
                    codes = MergeExtendedClass(codes, j)
                    // Add parent constructor calls
                    codes = AddParentConstructorCalls(codes)

                    break
                }
            }
        }

        // Test
        //TestMergeExtendedClass = codes
        //TestAddParentConstructorCalls = codes
        
        return codes
    }

    /// <summary>
    /// For each class, add parent constructor calls to the constructor
    /// </summary>
    public static func AddParentConstructorCalls(
        _ inputCodes: [String]
    ) -> [String] {

        var codeList = inputCodes

        var i = 0
        while i < codeList.count {

            // Look for inheritance symbol ":"
            if codeList[i] != ":" {
                i += 1
                continue
            }

            // Derived class
            let normalizedDerivedClassName =
                codeList[i - 1].replacingOccurrences(
                    of: PH_RESV_KW_CLASS,
                    with: ""
                )

            let splitDerived =
                normalizedDerivedClassName.split(separator: ".").map(String.init)
            let actualDerivedClassName =
                splitDerived.last ?? normalizedDerivedClassName

            // Parent class
            let normalizedParentClassName = codeList[i + 1]
            let splitParent =
                normalizedParentClassName.split(separator: ".").map(String.init)
            let actualParentClassName =
                splitParent.last ?? normalizedParentClassName

            // Find class scope
            let scopeBlockStartIndex = i - 1
            let scopeBlockEndIndex =
                FindEndScopeBlock(
                    codeList,
                    scopeBlockStartIndex,
                    "{",
                    "}"
                )

            if scopeBlockEndIndex == -1 {
                i += 1
                continue
            }

            let constructorName =
                normalizedDerivedClassName + "." + actualDerivedClassName
            let constructorToken =
                PH_RESV_KW_FUNCTION_VOID + constructorName

            var j = scopeBlockStartIndex
            while j < scopeBlockEndIndex {

                if codeList[j] == constructorToken {

                    // Ignore constructors with parameters
                    if j + 2 >= codeList.count || codeList[j + 2] != ")" {
                        j += 1
                        continue
                    }

                    let callToParentConstructorCode =
                        normalizedDerivedClassName
                        + "."
                        + normalizedParentClassName
                        + "."
                        + actualParentClassName

                    // Already exists?
                    if j + 4 < codeList.count,
                       codeList[j + 4] == callToParentConstructorCode {
                        j += 1
                        continue
                    }

                    // Insert parent constructor call
                    let newCode: [String] = [
                        callToParentConstructorCode,
                        "(",
                        ")",
                        ";"
                    ]

                    codeList.insert(contentsOf: newCode, at: j + 4)
                    j += newCode.count
                }

                j += 1
            }

            i += 1
        }

        return codeList
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
    private static func GenerateClassListReorderedByDependancy(
        _ inputCodes: [String],
        _ isRemoveNonDependantClass: Bool = false
    ) -> [String] {

        // Get list of class names with its parent class separated by a colon
        // (e.g. "Test:ParentTest")
        let classDeclarationList =
            GetAllKeywordDeclarations(inputCodes, PH_RESV_KW_CLASS, true)

        // Bring base classes (classes without ":") to top of list
        var reorderedClasses: [String] = []
        var derivedClasses: [String] = []

        for cls in classDeclarationList {
            if cls.contains(":") {
                derivedClasses.append(cls)
            } else {
                reorderedClasses.append(cls)
            }
        }
        reorderedClasses.append(contentsOf: derivedClasses)

        // Reorder so that least dependent class comes first
        for i in 0..<reorderedClasses.count {

            if !reorderedClasses[i].contains(":") { continue }

            let classSep = reorderedClasses[i].split(separator: ":", maxSplits: 1)
            if classSep.count < 2 { continue }

            let parentClassToSearchFor = String(classSep[1])

            for j in 0..<reorderedClasses.count {

                if !reorderedClasses[j].contains(":") { continue }

                let parentClassSep = reorderedClasses[j].split(separator: ":", maxSplits: 1)
                if parentClassSep.count < 2 { continue }

                if parentClassSep[0] == parentClassToSearchFor {

                    // If parent already appears before derived class, OK
                    if j < i { break }

                    // Otherwise, move parent class above derived class
                    let parentClassDef = reorderedClasses[j]
                    reorderedClasses.remove(at: j)
                    reorderedClasses.insert(parentClassDef, at: i)
                    break
                }
            }
        }

        // Remove parent class part after colon
        for i in 0..<reorderedClasses.count {

            if let colonIndex = reorderedClasses[i].firstIndex(of: ":") {

                // Remove parent portion
                reorderedClasses[i] =
                    String(reorderedClasses[i][..<colonIndex])

            } else if isRemoveNonDependantClass {

                // Remove non-dependent classes if requested
                reorderedClasses[i] = ""
            }
        }

        // Remove blank entries
        reorderedClasses = reorderedClasses.filter {
            !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
        }

        return reorderedClasses
    }
    
    /// <summary>
    /// Merges parent class with derived class
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="startClassDefIndex">The line index in the line of codes where the class is defined</param>
    /// <returns></returns>
    public static func MergeExtendedClass(
        _ inputCodes: [String],
        _ startClassDefIndex: Int
    ) -> [String] {

        var codeList: [String] = inputCodes
        var startIndex = startClassDefIndex
        var derivedClassStartIndex = 0

        // --------------------------------------------------
        // Check inheritance ":"
        // --------------------------------------------------
        if startIndex + 1 < codeList.count, codeList[startIndex + 1] == ":" {

            startIndex += 1

            // Must be a normal class (not static)
            if !codeList[startIndex - 1].contains(PH_RESV_KW_CLASS) {
                return inputCodes
            }

            derivedClassStartIndex = startIndex - 1

        } else {

            // No inheritance, just add constructor
            if !codeList[startIndex].contains(PH_RESV_KW_CLASS) {
                return inputCodes
            }

            derivedClassStartIndex = startIndex

            let derivedClassName =
                codeList[derivedClassStartIndex]
                    .replacingOccurrences(of: PH_RESV_KW_CLASS, with: "")

            return AddConstructorToDerivedClass(
                codeList,
                startIndex,
                startIndex,
                derivedClassName,
                ""
            )
        }

        // --------------------------------------------------
        // Extract class names
        // --------------------------------------------------
        let derivedClassName =
            codeList[derivedClassStartIndex]
                .replacingOccurrences(of: PH_RESV_KW_CLASS, with: "")

        let parentClassName = codeList[startIndex + 1]

        // --------------------------------------------------
        // Generate member lists
        // --------------------------------------------------
        let derivedClassMemberList =
            GenerateScopeMemberList(inputCodes, derivedClassStartIndex)

        let parentClassMemberList =
            GenerateScopeMemberList(
                inputCodes,
                0,
                PH_RESV_KW_CLASS + parentClassName
            )

        // --------------------------------------------------
        // Check parent class existence
        // --------------------------------------------------
        var isParentClassExist = false
        for c in inputCodes {
            if c == PH_RESV_KW_CLASS + parentClassName {
                isParentClassExist = true
                break
            }
        }
        if !isParentClassExist {
            return inputCodes
        }

        // --------------------------------------------------
        // Merge parent members
        // --------------------------------------------------
        let sortedParentMembers =
            parentClassMemberList.sorted { $0.key < $1.key }

        for (_, parentMemberValue) in sortedParentMembers {

            let declareKeyword = parentMemberValue.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            let isScopeBlock =
                !declareKeyword.isEmpty &&
                NonVariableDeclareKeywords.contains(declareKeyword[0] + " ")

            let startMemberCodeStart =
                parentClassMemberList.first { $0.value == parentMemberValue }!.key

            let endMemberCodeStart: Int =
                isScopeBlock
                ? FindEndScopeBlock(inputCodes, startMemberCodeStart, "{", "}")
                : FindEndScopeBlock(inputCodes, startMemberCodeStart, "", ";")

            if endMemberCodeStart == -1 { continue }

            // Extract member name
            let parentSplit = parentMemberValue.split(separator: ".").map(String.init)
            let parentMemberName = parentSplit.last ?? ""

            // Check duplicate member name
            var isSameMemberNameExists = false
            for (_, derivedMemberValue) in derivedClassMemberList {
                let derivedSplit = derivedMemberValue.split(separator: ".").map(String.init)
                if derivedSplit.last == parentMemberName {
                    isSameMemberNameExists = true
                    break
                }
            }

            var newCodeList: [String] = []

            if isSameMemberNameExists {

                // Duplicate exists → Derived.Parent.member
                for k in startMemberCodeStart...endMemberCodeStart {
                    newCodeList.append(
                        ReplaceFirstOccurance(
                            inputCodes[k],
                            parentClassName,
                            derivedClassName + "." + parentClassName
                        )
                    )
                }

            } else {

                // No duplicate → Derived.member
                for k in startMemberCodeStart...endMemberCodeStart {
                    newCodeList.append(
                        ReplaceFirstOccurance(
                            inputCodes[k],
                            parentClassName,
                            derivedClassName
                        )
                    )
                }

                // Also add Derived.Parent.member
                for k in startMemberCodeStart...endMemberCodeStart {
                    newCodeList.append(
                        ReplaceFirstOccurance(
                            inputCodes[k],
                            parentClassName,
                            derivedClassName + "." + parentClassName
                        )
                    )
                }
            }

            // Insert merged members after class header
            codeList.insert(
                contentsOf: newCodeList,
                at: startIndex + 3
            )
        }

        // --------------------------------------------------
        // Rebuild codes (indexes changed)
        // --------------------------------------------------
        let rebuiltCodes = codeList

        return AddConstructorToDerivedClass(
            rebuiltCodes,
            startIndex,
            startIndex,
            derivedClassName,
            parentClassName
        )
    }
    
    public static func AddConstructorToDerivedClass(
        _ codeList: [String],
        _ startClassDefIndex: Int,
        _ derivedClassStartIndex: Int,
        _ derivedClassName: String,
        _ parentClassName: String
    ) -> [String] {

        var codeList = codeList

        // --------------------------------------------------
        // Generate class member list for derived class
        // --------------------------------------------------
        let derivedClassMemberList =
            GenerateScopeMemberList(
                codeList,
                derivedClassStartIndex
            )

        // Split derived class name to get actual class name
        let splitDerivedClassName =
            derivedClassName.split(separator: ".").map(String.init)
        let actualDerivedClassName =
            splitDerivedClassName.last ?? derivedClassName

        let derivedClassConstructorCode =
            PH_RESV_KW_FUNCTION_VOID
            + derivedClassName
            + "."
            + actualDerivedClassName

        // --------------------------------------------------
        // Check if derived class already has a constructor
        // (with no parameters)
        // --------------------------------------------------
        var isDerivedClassConstructorExists = false

        for (index, value) in derivedClassMemberList {

            if value == derivedClassConstructorCode {

                // Check if constructor has no parameters → ")"
                if index + 2 < codeList.count,
                   codeList[index + 2] == ")" {
                    isDerivedClassConstructorExists = true
                    break
                }
            }
        }

        // --------------------------------------------------
        // Create default constructor if missing
        // --------------------------------------------------
        if !isDerivedClassConstructorExists {

            var newConstructorCodeList: [String] = []

            newConstructorCodeList.append(derivedClassConstructorCode)
            newConstructorCodeList.append("(")
            newConstructorCodeList.append(")")
            newConstructorCodeList.append("{")

            // Call parent constructor if exists
            if parentClassName != "" {

                let splitParentClassName =
                    parentClassName.split(separator: ".").map(String.init)
                let actualParentClassName =
                    splitParentClassName.last ?? parentClassName

                newConstructorCodeList.append(
                    derivedClassName
                    + "."
                    + parentClassName
                    + "."
                    + actualParentClassName
                )
                newConstructorCodeList.append("(")
                newConstructorCodeList.append(")")
                newConstructorCodeList.append(";")
            }

            // Add default return(0);
            newConstructorCodeList.append(PH_RESV_KW_RETURN.trimmingCharacters(in: .whitespacesAndNewlines))
            newConstructorCodeList.append("(")
            newConstructorCodeList.append("0")
            newConstructorCodeList.append(")")
            newConstructorCodeList.append(";")
            newConstructorCodeList.append("}")

            // --------------------------------------------------
            // Insert constructor into class body
            // --------------------------------------------------
            var insertIndex = startClassDefIndex + 3
            if parentClassName == "" {
                insertIndex = startClassDefIndex + 2
            }

            codeList.insert(
                contentsOf: newConstructorCodeList,
                at: insertIndex
            )
        }

        return codeList
    }
    
    /// <summary>
    /// Generates list of members defined within the specified scope (i.e. member list inside a class {...})
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="startCodeIndex"></param>
    /// <param name="scopeName"></param>
    /// <returns></returns>
    public static func GenerateScopeMemberList(
        _ codes: [String],
        _ startCodeIndex: Int,
        _ scopeName: String = ""
    ) -> [Int: String] {

        // --------------------------------------------------
        // Get all class names
        // --------------------------------------------------
        var classList =
            GetAllKeywordDeclarations(
                codes,
                PH_RESV_KW_CLASS,
                false
            )

        // Append trailing space (C# behavior)
        for i in 0..<classList.count {
            classList[i] += " "
        }

        // --------------------------------------------------
        // Merge class member types (variables + functions)
        // --------------------------------------------------
        var mergedClassMemberTypeList =
            ClassMemberReservedKeyWords

        mergedClassMemberTypeList.append(contentsOf: classList)

        var retList: [Int: String] = [:]

        // --------------------------------------------------
        // Locate scope name if specified
        // --------------------------------------------------
        var startIndex = startCodeIndex

        if scopeName != "" {
            for i in 0..<codes.count {
                if codes[i] == scopeName {
                    startIndex = i
                    break
                }
            }
        }

        // --------------------------------------------------
        // Find end of scope block
        // --------------------------------------------------
        let endScopeBlockIndex =
            FindEndScopeBlock(
                codes,
                startIndex,
                "{",
                "}"
            )

        if endScopeBlockIndex == -1 {
            return retList
        }

        // --------------------------------------------------
        // Walk through scope and collect members
        // --------------------------------------------------
        var i = startIndex
        while i < endScopeBlockIndex {

            let keywordToCheck =
                codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            if keywordToCheck.count < 2 {
                i += 1
                continue
            }

            let keyword = keywordToCheck[0] + " "

            let isKnownMemberType = mergedClassMemberTypeList.contains(keyword)
            let isVariableDeclaration =
                keywordToCheck.count >= 2 &&
                codes[i + 1 < codes.count ? i + 1 : i].contains(";")

            if !isKnownMemberType && !isVariableDeclaration {
                i += 1
                continue
            }

            // --------------------------------------------------
            // Function found → skip entire function block
            // --------------------------------------------------
            if AllFunctionKeywords.contains(keyword) {

                let endFunctionBlockIndex =
                    FindEndScopeBlock(
                        codes,
                        i,
                        "{",
                        "}"
                    )

                retList[i] = codes[i]

                if endFunctionBlockIndex != -1 {
                    i = endFunctionBlockIndex
                }

                i += 1
                continue
            }

            // --------------------------------------------------
            // Variable / member declaration
            // --------------------------------------------------
            retList[i] = codes[i]
            i += 1
        }

        return retList
    }
    
    /// <summary>
    /// Adds constructors to static class
    /// In C#, you cannot add constructors to static classes,
    /// but we add it in so all class member initiations can be done
    /// in this constructor later on
    /// </summary>
    public static func AddConstructorsToStaticClass(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        var i = 0
        while i < codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count != 2 {
                i += 1
                continue
            }

            // "static_class "
            if splitCode[0] + " " != PH_RESV_KW_STATIC_CLASS {
                i += 1
                continue
            }

            // Class name may include namespace: A.B.C
            let splitClassName = splitCode[1].split(separator: ".").map(String.init)
            guard let classNameWithoutScope = splitClassName.last else {
                i += 1
                continue
            }

            // Insert constructor right after "{"
            // Safe because static class cannot have constructor in C#
            if i + 1 < codes.count {
                codes[i + 1] =
                    "{"
                    + PH_RESV_KW_FUNCTION_VOID
                    + splitCode[1]
                    + "."
                    + classNameWithoutScope
                    + "(){ return(0); }"
            }

            i += 1
        }

        // Clean up code (same behavior as C#)
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Converts for loops to while loops
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func PreProcessForBlock(_ inputCodes: [String]) -> [String] {
        var codes = inputCodes
        var i = 0

        while i < codes.count {
            if codes[i] == PH_RESV_KW_FOR {
                codes = ConvertForToWhileLoop(codes, i)

                // Since position of "while" statement has been shifted,
                // search for the next FOR and replace it with WHILE
                var j = i
                while j < codes.count {
                    if codes[j] == PH_RESV_KW_FOR {
                        codes[j] = PH_RESV_KW_WHILE
                        i = j
                        break
                    }
                    j += 1
                }
            }
            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Processes the following for a single while loop block:
    /// Place the equation for logical comparison inside while statement outside the while brackets,
    /// and also place a label for the start and end block of the while statement
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="startCodeIndex"></param>
    /// <returns></returns>
    public static func ConvertForToWhileLoop(
        _ codes: [String],
        _ startCodeIndex: Int
    ) -> [String] {

        var codeList = codes

        // --------------------------------------------------
        // Find parent scope block
        // --------------------------------------------------
        var startParentScopeBlock = 0
        var i = startCodeIndex
        while i >= 0 {
            if codeList[i] == "{" {
                startParentScopeBlock = i
                break
            }
            i -= 1
        }

        _ =
            FindEndScopeBlock(codeList, startParentScopeBlock, "{", "}")

        // --------------------------------------------------
        // Find for(...) and its scope
        // --------------------------------------------------
        let forConditionalStatementEnd =
            FindEndScopeBlock(codeList, startCodeIndex + 1, "(", ")")

        let scopeBlockEndIndex =
            FindEndScopeBlock(codeList, forConditionalStatementEnd + 1, "{", "}")

        // --------------------------------------------------
        // Extract initializer
        // --------------------------------------------------
        var initializer = ""
        var equationIndex = 0

        i = startCodeIndex + 2
        while i < forConditionalStatementEnd {
            initializer += codeList[i]
            codeList[i] = ""

            if codes[i] == ";" {
                var j = i + 1
                while j < forConditionalStatementEnd {
                    if codes[j] == ";" {
                        codeList[j] = ""
                        equationIndex = j + 1
                    }
                    j += 1
                }
                break
            }
            i += 1
        }

        // --------------------------------------------------
        // Extract loop equation
        // --------------------------------------------------
        var loopEquation = ""
        i = equationIndex
        while i < forConditionalStatementEnd {
            loopEquation += codes[i]
            codeList[i] = ""
            i += 1
        }
        loopEquation += ";"

        // --------------------------------------------------
        // Insert initializer and loop equation
        // --------------------------------------------------
        codeList.insert(initializer, at: startCodeIndex)
        codeList.insert(loopEquation, at: scopeBlockEndIndex + 1)

        // --------------------------------------------------
        // Rewrite continue inside for loop
        // --------------------------------------------------
        i = forConditionalStatementEnd + 3
        while i < scopeBlockEndIndex + 2 {

            // Ignore nested loops
            if codeList[i] == PH_RESV_KW_WHILE || codeList[i] == PH_RESV_KW_FOR {
                let endNested =
                    FindEndScopeBlock(codeList, i, "{", "}")
                i = endNested + 1
                continue
            }

            // for-continue executes loop equation
            if codeList[i] == PH_RESV_KW_CONTINUE {
                codeList[i] = loopEquation + PH_RESV_KW_CONTINUE
            }

            i += 1
        }

        // --------------------------------------------------
        // Re-tokenize
        // --------------------------------------------------
        let newCodes = codeList.joined()
        return splitCodesToArray(newCodes)
    }
    
    /// Preprocess while loop statements by converting them to "if" statements
    /// and adding labels for start and end of the while block
    public static func PreProcessWhileBlock(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var i = 0

        while i < codes.count {

            if codes[i] == PH_RESV_KW_WHILE {

                codes = SeparateWhileExpressionsAndAddLabel(codes, i)

                // Since the position of "while" has shifted,
                // find the next while and convert it to if
                var j = i
                while j < codes.count {
                    if codes[j] == PH_RESV_KW_WHILE {
                        codes[j] = PH_RESV_KW_IF
                        i = j
                        break
                    }
                    j += 1
                }
            }

            i += 1
        }

        return codes
    }
    
    /// Converts a single while block into an if + goto + label structure
    public static func SeparateWhileExpressionsAndAddLabel(
        _ codes: [String],
        _ startCodeIndex: Int
    ) -> [String] {

        // C#: List<string> codeList = new List<string>();
        var codeList = codes

        // --------------------------------------------------
        // Find parent scope block "{"
        // --------------------------------------------------
        var startParentScopeBlock = 0
        var i = startCodeIndex
        while i >= 0 {
            if codeList[i] == "{" {
                startParentScopeBlock = i
                break
            }
            i -= 1
        }

        _ = FindEndScopeBlock(
            codeList,
            startParentScopeBlock,
            "{",
            "}"
        )

        // --------------------------------------------------
        // Unique while ID
        // --------------------------------------------------
        let whileID =
            RandomString(
                RND_ALPHABET_STRING_LEN_MAX,
                true,
                TmpVarIdentifier
            )

        // --------------------------------------------------
        // Locate while(condition) and its scope
        // --------------------------------------------------
        let logicalComparisonEQ =
            FindEndScopeBlock(
                codeList,
                startCodeIndex + 1,
                "(",
                ")"
            )

        let scopeBlockEndIndex =
            FindEndScopeBlock(
                codeList,
                logicalComparisonEQ + 1,
                "{",
                "}"
            )

        // --------------------------------------------------
        // Rewrite while keyword with begin label
        // --------------------------------------------------
        codeList[startCodeIndex] =
            PH_ID + PH_LABEL + PH_ID
            + "while_block_begin_" + whileID
            + ";"
            + PH_RESV_KW_WHILE

        // --------------------------------------------------
        // Rewrite closing brace with goto + end label
        // --------------------------------------------------
        codeList[scopeBlockEndIndex] =
            PH_RESV_KW_GOTO
            + PH_ID + PH_LABEL + PH_ID
            + "while_block_begin_" + whileID
            + ";"
            + "} "
            + PH_ID + PH_LABEL + PH_ID
            + "while_block_end_" + whileID
            + ";"

        // --------------------------------------------------
        // Rewrite break / continue inside while block
        // --------------------------------------------------
        i = logicalComparisonEQ + 2
        while i < scopeBlockEndIndex {

            defer { i += 1 }   // ★ C# for(i++) 完全再現

            // Ignore nested while blocks
            if codeList[i] == PH_RESV_KW_WHILE {
                let endNestedWhileBlock =
                    FindEndScopeBlock(
                        codeList,
                        i,
                        "{",
                        "}"
                    )
                i = endNestedWhileBlock
                continue
            }

            if codeList[i] == PH_RESV_KW_BREAK {

                codeList[i] =
                    PH_RESV_KW_GOTO
                    + PH_ID + PH_LABEL + PH_ID
                    + "while_block_end_" + whileID

            } else if codeList[i] == PH_RESV_KW_CONTINUE {

                codeList[i] =
                    PH_RESV_KW_GOTO
                    + PH_ID + PH_LABEL + PH_ID
                    + "while_block_begin_" + whileID
            }
        }

        // --------------------------------------------------
        // Re-tokenize (C# と同じ join → split)
        // --------------------------------------------------
        let newCodes = codeList.joined()
        return splitCodesToArray(newCodes)
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
    public static func PreProcessIfElseBlock(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var i = 0

        while i < codes.count {

            if codes[i] == PH_RESV_KW_IF {

                codes = SeparateIfElseExpressionsAndAddLabel(codes, i)

                // Re-scan because indexes are shifted
                var j = i
                while j < codes.count {
                    if codes[j] == PH_RESV_KW_IF {
                        i = j
                        break
                    }
                    j += 1
                }
            }

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Processes the following for a single if/else if/else block:
    /// Place the equation for logical comparison inside if/else if statements outside the if/else if brackets,
    /// and also place a label for the start and end block of the if statement
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="startCodeIndex"></param>
    /// <returns></returns>
    public static func SeparateIfElseExpressionsAndAddLabel(
        _ codes: [String],
        _ startCodeIndex: Int
    ) -> [String] {

        var codeList = codes

        // --------------------------------------------------
        // Find parent scope
        // --------------------------------------------------
        var startParentScopeBlock = 0
        var i = startCodeIndex
        while i >= 0 {
            if codeList[i] == "{" {
                startParentScopeBlock = i
                break
            }
            i -= 1
        }

        _ =
            FindEndScopeBlock(codeList, startParentScopeBlock, "{", "}")

        // --------------------------------------------------
        // Unique ID
        // --------------------------------------------------
        let ifID =
            RandomString(
                RND_ALPHABET_STRING_LEN_MAX,
                true,
                TmpVarIdentifier
            )

        // --------------------------------------------------
        // Extract IF condition
        // --------------------------------------------------
        var logicalComparisonEQ =
            FindEndScopeBlock(codeList, startCodeIndex + 1, "(", ")")

        var scopeBlockEndIndex =
            FindEndScopeBlock(codeList, logicalComparisonEQ + 1, "{", "}")

        var tmpVar =
            RandomString(
                RND_ALPHABET_STRING_LEN_MAX,
                true,
                TmpVarIdentifier
            )

        // Move condition outside IF
        codeList[startCodeIndex + 1] = PH_RESV_KW_BYTE + tmpVar + "="
        codeList[logicalComparisonEQ] = ";"
        codeList.insert(PH_RESV_KW_IF + "(" + tmpVar + ")", at: logicalComparisonEQ + 1)
        codeList.remove(at: startCodeIndex)

        // Labels
        codeList[logicalComparisonEQ - 1] =
            ";" + PH_ID + PH_LABEL + PH_ID + "if_block_begin_" + ifID + ";"
            + PH_ID + PH_LABEL + PH_ID + "if_begin_" + ifID + ";"

        codeList[scopeBlockEndIndex] =
            "} " + PH_ID + PH_LABEL + PH_ID + "if_end_" + ifID + ";"

        var elseifCounter = 0
        var endOfIfElseBlockIndex = -1

        // --------------------------------------------------
        // ELSE / ELSE IF chain
        // --------------------------------------------------
        var scanIndex = scopeBlockEndIndex + 1

        while scanIndex < codes.count {

            if codes[scanIndex] == PH_RESV_KW_ELSE {

                codeList[scopeBlockEndIndex] +=
                    PH_ID + PH_LABEL + PH_ID + "else_begin_" + ifID + ";"

                endOfIfElseBlockIndex =
                    FindEndScopeBlock(codes, scanIndex, "{", "}")

                codeList[endOfIfElseBlockIndex] =
                    "} " + PH_ID + PH_LABEL + PH_ID + "else_end_" + ifID + ";"
                    + PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";"

                break
            }

            if codes[scanIndex] == PH_RESV_KW_ELSEIF {

                logicalComparisonEQ =
                    FindEndScopeBlock(codes, scanIndex + 1, "(", ")")

                let elseifScopeEnd =
                    FindEndScopeBlock(codes, logicalComparisonEQ + 1, "{", "}")

                tmpVar =
                    RandomString(
                        RND_ALPHABET_STRING_LEN_MAX,
                        true,
                        TmpVarIdentifier
                    )

                codeList[scanIndex + 1] = PH_RESV_KW_BYTE + tmpVar + "="
                codeList[logicalComparisonEQ] = ";"
                codeList.insert(
                    PH_RESV_KW_ELSEIF + "(" + tmpVar + ")",
                    at: logicalComparisonEQ + 1
                )
                codeList.remove(at: scanIndex)

                codeList[logicalComparisonEQ - 1] =
                    ";" + PH_ID + PH_LABEL + PH_ID
                    + "elseif_begin_\(elseifCounter)_\(ifID);"

                codeList[elseifScopeEnd] =
                    "} " + PH_ID + PH_LABEL + PH_ID
                    + "elseif_end_\(elseifCounter)_\(ifID);"

                elseifCounter += 1
                scopeBlockEndIndex = elseifScopeEnd
                scanIndex = elseifScopeEnd + 1
                continue
            }

            break
        }

        if endOfIfElseBlockIndex == -1 {
            codeList[scopeBlockEndIndex] +=
                PH_ID + PH_LABEL + PH_ID + "if_block_end_" + ifID + ";"
        }

        // --------------------------------------------------
        // Re-tokenize
        // --------------------------------------------------
        let newCodes = codeList.joined()
        return splitCodesToArray(newCodes)
    }
    
    /// <summary>
    /// Resolve floating function calls that usually returns some kind of value back to the caller
    /// NOTE: Our functional always returns some kind of value, even with the void function - will return 0
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ResolveFloatingFunctionCalls(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        // --- Dependency placeholders ---
        let funcList = GetAllFunctionList(codes)
        let instanceObjList = GetInstanceObjects(codes)

        var i = 0
        while i < codes.count {

            let token = codes[i]

            // Split by space
            let splitCode = token.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count != 1 {
                i += 1
                continue
            }

            // Skip reserved keywords
            if AllReservedKeywords.contains(token + " ") {
                i += 1
                continue
            }

            // Remove array accessor: foo[0] → foo
            let actualFunction =
                token.replacingOccurrences(
                    of: #"\[(.*?)\]"#,
                    with: "",
                    options: .regularExpression
                )

            var resolvedFunction = actualFunction

            // Check function existence
            if funcList[resolvedFunction] == nil {

                // Possibly instance method call
                let parts = resolvedFunction.split(separator: ".").map(String.init)
                if parts.count <= 1 {
                    i += 1
                    continue
                }

                let instanceName = parts.dropLast().joined(separator: ".")
                let methodName = parts.last!

                var resolvedInstance = instanceName

                if instanceObjList[resolvedInstance] == nil {
                    resolvedInstance =
                        GetResolvedNestedInstanceVar(
                            resolvedInstance,
                            instanceObjList
                        )
                }

                guard let className = instanceObjList[resolvedInstance] else {
                    i += 1
                    continue
                }

                resolvedFunction = className + "." + methodName

                if funcList[resolvedFunction] == nil {
                    i += 1
                    continue
                }
            }

            // If assignment exists just before, skip
            if i > 0, codes[i - 1] == "=" {
                i += 1
                continue
            }

            // Check if inside expression
            var isInsideExpression = false
            var j = i - 1
            while j >= 0 {
                if codes[j] == "=" {
                    isInsideExpression = true
                }
                if codes[j] == ";" {
                    break
                }
                j -= 1
            }

            if isInsideExpression {
                i += 1
                continue
            }

            // Generate temp variable
            let tmpVar =
                RandomString(
                    RND_ALPHABET_STRING_LEN_MAX,
                    true,
                    TmpVarIdentifier
                )

            let returnType = funcList[resolvedFunction]!

            let assignmentCode: String
            if returnType + " " == PH_RESV_KW_VOID {
                assignmentCode =
                    PH_RESV_KW_BYTE
                    + tmpVar
                    + ";"
                    + tmpVar
                    + "="
            } else {
                assignmentCode =
                    returnType
                    + " "
                    + tmpVar
                    + ";"
                    + tmpVar
                    + "="
            }

            codes[i] = assignmentCode + codes[i]
            i += 1
        }

        // Clean up
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Gets all the functions in the code and return it in (function name, return type) Dictionary list
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func GetAllFunctionList(
        _ codes: [String]
    ) -> [String: String] {

        var funcList: [String: String] = [:]

        for i in 0..<codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.isEmpty { continue }

            // Check function keyword
            let keyword = splitCode[0] + " "
            if !AllFunctionKeywords.contains(keyword) {
                continue
            }

            // function_xxx → extract return type
            var funcSplit = splitCode[0].split(separator: "_").map(String.init)
            if funcSplit.count < 2 { continue }

            // If this is a function that returns a pointer
            if splitCode[0] == PH_RESV_KW_FUNCTION_PTR.trimmingCharacters(in: .whitespaces) {
                funcSplit[1] = PH_RESV_KW_PTR.trimmingCharacters(in: .whitespaces)
            }

            // splitCode[1] = fully qualified function name
            if splitCode.count >= 2 {
                funcList[splitCode[1]] = funcSplit[1]
            }
        }

        return funcList
    }
    
    /// <summary>
    /// Returns all the instance object found in the code, as <fullVarName, className>
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// Returns all the instance object found in the code, as <fullVarName, className>
    public static func GetInstanceObjects(
        _ codes: [String]
    ) -> [String: String] {

        var instanceObjList: [String: String] = [:]

        // --------------------------------------------------
        // Get all predefined + user class names
        // --------------------------------------------------
        var mergedList = ClassMemberReservedKeyWords

        let userClasses =
            GetAllKeywordDeclarations(
                codes,
                PH_RESV_KW_CLASS,
                false
            )

        mergedList.append(contentsOf: userClasses)

        let classDeclarationList =
            mergedList.map {
                $0.trimmingCharacters(in: .whitespacesAndNewlines)
            }

        // --------------------------------------------------
        // ① Find instantiated objects via "𒀭PH_NEW𒀭"
        // --------------------------------------------------
        let newKeyword = PH_ID + PH_NEW + PH_ID

        for i in 0..<codes.count {

            if !codes[i].contains(newKeyword) {
                continue
            }
            if i - 2 < 0 {
                continue
            }

            var varName = codes[i - 2]

            // Extract class name
            var className =
                codes[i].replacingOccurrences(
                    of: newKeyword,
                    with: ""
                )

            // Remove array suffix
            className = className.replacingOccurrences(
                of: #"\[(.*?)$"#,
                with: "",
                options: .regularExpression
            )

            if !classDeclarationList.contains(className) {
                continue
            }

            // Remove type if exists
            let splitCheck = varName.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCheck.count > 1 {
                varName = splitCheck[1]
            }

            // Remove array suffix from var name
            varName = varName.replacingOccurrences(
                of: #"\[(.*?)$"#,
                with: "",
                options: .regularExpression
            )

            if instanceObjList[varName] == nil {
                instanceObjList[varName] = className
            }
        }

        // --------------------------------------------------
        // ② User-created class instance declarations
        //    e.g. ClassName obj;
        // --------------------------------------------------
        for code in codes {

            let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count < 2 {
                continue
            }

            let typeName = splitCode[0]
            let varName = splitCode[1]

            if !userClasses.contains(typeName) {
                continue
            }

            if instanceObjList[varName] == nil {
                instanceObjList[varName] = typeName
            }
        }

        // --------------------------------------------------
        // ③ Pointer variables treated as instance objects
        // --------------------------------------------------
        for code in codes {

            let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count < 2 {
                continue
            }

            if splitCode[0] + " " == PH_RESV_KW_PTR {

                let varName = splitCode[1]
                if instanceObjList[varName] == nil {
                    instanceObjList[varName] = splitCode[0]
                }
            }
        }

        return instanceObjList
    }
    
    /// <summary>
    /// Recursively resolve the actual class members that the variable is referring
    /// N.Program.Main.a.X      -> N.INT32.X
    /// N.Program.Main.a.X.X    -> N.INT32.X.X -> N.INT32.X
    /// N.Program.Main.a.X.Y    -> N.INT32.X.Y -> N.INT32.Y
    /// </summary>
    /// <param name="varToResolve">
    /// Prerequisite: All variables' scope needs to be resolved
    /// </param>
    /// <param name="instanceObjList">
    /// Generated using GetInstanceObjects(codes)
    /// </param>
    /// <returns></returns>
    public static func GetResolvedNestedInstanceVar(
        _ varToResolve: String,
        _ instanceObjList: [String: String]
    ) -> String {

        let originalVar = varToResolve

        // Split by dot and extract the final member
        let sep = varToResolve.split(separator: ".").map(String.init)
        guard let member = sep.last else {
            return varToResolve
        }

        // Rebuild var without touching member yet
        var resolvedVar = sep.joined(separator: ".")

        // --------------------------------------------------
        // Repeat replacement until no more changes occur
        // (C# goto repeatVarResolveSearch equivalent)
        // --------------------------------------------------
        var didReplace = true

        while didReplace {
            didReplace = false

            for (key, value) in instanceObjList {
                if resolvedVar.contains(key) {
                    resolvedVar = resolvedVar.replacingOccurrences(of: key, with: value)
                    didReplace = true
                    break   // restart scan (same as goto)
                }
            }
        }

        // If resolution changed, re-append the member
        if originalVar != resolvedVar {
            resolvedVar += "." + member
        }

        return resolvedVar
    }
    
    /// <summary>
    /// Convert all char placeholders to actual decimal (UTF-16 / Decimal) representation
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ConvertCharPlaceHolderToDecimal(
        _ inputCodes: [String]
    ) -> [String] {

        var codeList = inputCodes

        for i in 0..<codeList.count {

            // C#: if(codeList[i].IndexOf(PH_ID + PH_CHAR) == -1) continue;
            if !codeList[i].contains(PH_ID + PH_CHAR) {
                continue
            }

            // Get original char literal (e.g. "'A'")
            guard var str = CharLiteralList[codeList[i]] else {
                continue
            }

            // Remove surrounding single quotes
            // C#: str = str.Substring(1, str.Length - 2);
            if str.count >= 2 {
                str = String(str.dropFirst().dropLast())
            }

            // Convert escape sequences
            str = str.replacingOccurrences(of: "\\\\", with: "\\")
            str = str.replacingOccurrences(of: "\\n", with: "\n")
            str = str.replacingOccurrences(of: "\\t", with: "\t")
            str = str.replacingOccurrences(of: "\\0", with: "\0")

            // Take first character and convert to UTF-16 value
            if let scalar = str.unicodeScalars.first {
                let value = UInt16(scalar.value)
                codeList[i] = String(value)
            }
        }

        return codeList
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
    public static func PreProcessFunctionParams(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var i = 0

        while i < codes.count {

            defer { i += 1 } // emulate C# for-loop increment even when continuing

            // Check if this token contains an assignment operator "="
            if !codes[i].contains("=") {
                continue
            }

            // Ensure this is not part of a logical operator (e.g. <=, >=, ==, !=)
            // C#: !Regex.IsMatch(codes[i - 1], @"[<|>|!]") && codes[i + 1] != "="
            if i - 1 < 0 { continue }
            if codes[i - 1].range(of: #"[<|>|!]"#, options: .regularExpression) != nil { continue }
            if i + 1 >= codes.count { continue }
            if codes[i + 1] == "=" { continue }

            // Check if this assignment is followed by a function call:
            // [i]   =
            // [i+1] functionName
            // [i+2] (
            if i + 2 >= codes.count || codes[i + 2] != "(" {
                continue
            }

            // Ignore function calls with no parameters
            if i + 3 < codes.count, codes[i + 3] == ")" {
                continue
            }

            // Ignore special intrinsic function calls (handled separately)
            if i + 1 < codes.count, codes[i + 1] == PH_RESV_KW_IASM_FUNCTION_CALL {
                continue
            }

            // --- Locate the beginning of the expression ---
            var expressionBeginIndex = 0
            var j = i
            while j >= 0 {
                if codes[j].range(of: #"[;|\}|\{]"#, options: .regularExpression) != nil
                    || codes[j].isEmpty {

                    expressionBeginIndex = j + 1

                    // If a temporary variable declaration exists just before, include it
                    if codes[j] == ";",
                       j - 1 >= 0,
                       codes[j - 1].contains(codes[expressionBeginIndex]),
                       codes[j - 1].contains(TmpVarIdentifier) {

                        expressionBeginIndex = j - 1
                    }
                    break
                }
                j -= 1
            }

            // --- Locate the end of the expression ---
            var expressionEndIndex = 0
            var roundBracketCnt = 0
            let funcRoundBracketStart = i + 2
            var funcRoundBracketEnd = 0

            j = i
            while j < codes.count {

                defer { j += 1 } // emulate C# for-loop increment

                // Ignore semicolons inside round brackets
                if codes[j] == "(" {
                    roundBracketCnt += 1
                    continue
                }
                if codes[j] == ")" {
                    roundBracketCnt -= 1
                    if roundBracketCnt == 0 {
                        funcRoundBracketEnd = j
                    }
                    continue
                }
                if codes[j] == ";" && roundBracketCnt == 0 {
                    expressionEndIndex = j - 1
                    break
                }
            }

            // --- Extract full expression and function parameters ---
            var expression = ""
            var parameters = ""

            if expressionBeginIndex <= expressionEndIndex {
                for k in expressionBeginIndex...expressionEndIndex {
                    expression += codes[k]
                    if k > funcRoundBracketStart && k < funcRoundBracketEnd {
                        parameters += codes[k]
                        // Remove all parameter expressions
                        codes[k] = ""
                    }
                }
            }

            // --- Normalize parameters into standalone assignments ---
            var normalizedParamCode = ""
            let tmpVar = RandomString(
                RND_ALPHABET_STRING_LEN_MAX,
                true,
                TmpVarIdentifier
            )

            // Temporarily replace commas inside nested parentheses so we can split params correctly
            // C#: Regex.Replace(parameters, @",(?=((?!\().)*?\))", tmpVar)
            let convertedParameters =
                parameters.replacingOccurrences(
                    of: #",(?=((?!\().)*?\))"#,
                    with: tmpVar,
                    options: .regularExpression
                )

            var splitParams = convertedParameters.split(separator: ",", omittingEmptySubsequences: false).map(String.init)

            for pIndex in 0..<splitParams.count {

                // Restore placeholders back to commas
                splitParams[pIndex] = splitParams[pIndex].replacingOccurrences(of: tmpVar, with: ",")

                var param = splitParams[pIndex]

                // If the single parameter contains an equal sign, it is already an assignment expression
                if param.contains("=") {
                    param += ";"
                } else {
                    param =
                        PH_RESV_KW_VAR
                        + tmpVar
                        + String(pIndex)
                        + "="
                        + param
                        + ";"
                }

                normalizedParamCode += param

                // Get the variable name that is declared
                let declaredVarName =
                    param.firstMatchString(
                        #" [a-zA-Z0-9_](.*?)(?=[^a-zA-Z0-9_\.])"#
                    ).trimmingCharacters(in: .whitespacesAndNewlines)

                // Set the declared var to the parameter, separated by comma
                if funcRoundBracketStart + 1 < codes.count {
                    codes[funcRoundBracketStart + 1] +=
                        declaredVarName
                        + (pIndex != splitParams.count - 1 ? "," : "")
                }
            }

            // Move the variable declarations BEFORE the function call
            codes[expressionBeginIndex] = normalizedParamCode + codes[expressionBeginIndex]

            // Move search to the end of function call statement.
            // In C#: i = funcRoundBracketEnd + 1; and then for-loop i++ happens.
            // Here, defer { i += 1 } will still run, matching C# behavior.
            i = funcRoundBracketEnd + 1
            continue
        }

        // Remove empty tokens
        codes = codes.filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }

        // Re-tokenize and normalize the code
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Convert expressions so that we can convert the expression into Reverse Polish Notation (RPN)
    ///
    /// Example:
    /// namespace N {
    ///     class Exp {
    ///         public Exp() {
    ///             int a = 1;
    ///             int result = (add(add(1, 2), 3) * (a - 5));
    ///         }
    ///         public int add(int a, int b) {
    ///             return a + b;
    ///         }
    ///     }
    /// }
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func PreProcessExpressions(_ inputCodes: [String]) -> [String] {

        var codes = inputCodes

        var i = 0
        while i < codes.count {

            // If we find an equal sign "=", check if it is an assignment operator
            if codes[i].contains("=") {

                // Check if not a logical operator (<=, >=, !=, ==)
                if i - 1 >= 0,
                   codes[i - 1].range(of: "[<|>|!]", options: .regularExpression) == nil,
                   i + 1 < codes.count,
                   codes[i + 1] != "=" {

                    // Trace back until the last statement ";" or "}" or "{"
                    var expressionBeginIndex = 0
                    var j = i
                    while j >= 0 {
                        if codes[j].range(of: "[;|\\}|\\{]", options: .regularExpression) != nil
                            || codes[j].isEmpty {
                            expressionBeginIndex = j + 1
                            break
                        }
                        j -= 1
                    }

                    var expressionEndIndex = 0
                    var roundBracketCnt = 0

                    // Get the end of expression
                    j = i
                    while j < codes.count {
                        defer { j += 1 }

                        if codes[j] == "(" {
                            roundBracketCnt += 1
                            continue
                        }

                        if codes[j] == ")" {
                            roundBracketCnt -= 1
                            continue
                        }

                        if codes[j] == ";" && roundBracketCnt == 0 {
                            expressionEndIndex = j - 1
                            break
                        }
                    }

                    // Build expression string
                    var expression = ""
                    if expressionBeginIndex <= expressionEndIndex {
                        for k in expressionBeginIndex...expressionEndIndex {
                            expression += codes[k]
                        }
                    }

                    // Separate type declaration (if any)
                    var typeDeclareStr = ""
                    if let spaceIndex = expression.firstIndex(of: " ") {
                        let idx = expression.distance(from: expression.startIndex, to: spaceIndex)
                        typeDeclareStr = String(expression.prefix(idx + 1))
                        expression = String(expression.dropFirst(idx + 1))
                    }

                    // Break down expression (actual logic to be implemented later)
                    let processedExpression = BreakDownExpression(expression, codes)

                    // Separate variable declaration and assignment
                    if !typeDeclareStr.isEmpty {
                        codes[expressionBeginIndex] =
                            codes[expressionBeginIndex] + ";" + processedExpression
                    } else {
                        codes[expressionBeginIndex] = processedExpression
                    }

                    // Clear old tokens
                    if expressionBeginIndex + 1 <= expressionEndIndex + 1 {
                        for k in (expressionBeginIndex + 1)...(expressionEndIndex + 1) {
                            if k < codes.count {
                                codes[k] = ""
                            }
                        }
                    }
                }
            }

            i += 1
        }

        // Remove empty tokens
        codes = codes.filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }

        // Regenerate clean code
        codes = RegenerateCleanCode(codes)

        return codes
    }

    /// <summary>
    /// Convert expressions so that we can convert the expression into Reverse Polish Notation (RPN)
    ///
    /// Example:
    /// namespace N {
    ///     class Exp {
    ///         public Exp() {
    ///             int a = 1;
    ///             int result = (add(add(1, 2), 3) * (a - 5));  // Convert this to RPN
    ///         }
    ///         public int add(int a, int b) {
    ///             return a + b;
    ///         }
    ///     }
    /// }
    /// </summary>
    /// <param name="expression">The expression string to break down</param>
    /// <param name="codes">Full tokenized code list (used for resolving instantiations)</param>
    /// <returns>Flattened string result that represents the broken down expression</returns>
    public static func BreakDownExpression(
        _ expression: String,
        _ codes: [String]
    ) -> String {
        // Replace sequential function call scope separator "." with placeholder "#",
        // to distinguish between variables and function call chaining.
        let seqFuncCallPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

        var exp = expression
        exp = exp.replacingOccurrences(of: ").", with: ")#")

        // Replace variable scope separator "." with a placeholder, because symbols within variables
        // are not allowed when parsing to RPN.
        let scopeSepPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)
        exp = exp.replacingOccurrences(of: ".", with: scopeSepPH)

        // Remove all spaces in the expression
        exp = exp.replacingOccurrences(of: " ", with: "")

        let incrementPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)
        let decrementPH = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

        // Convert increment/decrement placeholders (compiler uses full-width markers internally)
        if exp.contains("＋") && exp.contains("—") {
            // NOTE: int d = b+++a + 2; // C# interprets this as b++ + a + 2
            exp = exp.replacingOccurrences(of: "＋", with: incrementPH)
            exp = exp.replacingOccurrences(of: "—", with: decrementPH)
        }

        // Break down expressions containing arrays and functions into smaller expressions
        var results: [String] = BreakDownArrayFunc(exp)

        // Remove redundant code and optimize it
        results = EliminateRedundantExpressions(results)

        // Exceptional condition:
        // If we have a closing bracket "]" and an alphanumeric value straight after it,
        // it indicates that an array's function is called.
        // In that case, we may need to instantiate the temporary reference before usage.
        do {
            let pattern = #"\][a-zA-Z0-9]"#
            if exp.range(of: pattern, options: .regularExpression) != nil {

                for i in 0..<results.count {

                    // Extract variable name declared by "var"
                    let varPattern = "(?<=" + NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) + ").+?(?==)"
                    let variable = results[i].firstMatchString(varPattern)
                    if variable.isEmpty { continue }

                    // Extract right expression before the first "["
                    let rightPattern = "(?<==).+?(?=\\[)"
                    let rightExp = results[i].firstMatchString(rightPattern)
                    if rightExp.isEmpty { continue }

                    // Search another result that references "=variable{scopeSepPH}..."
                    for j in 0..<results.count {
                        let chkExp = results[j].firstMatchString(#"=.+?(?=;)"#)
                        if chkExp.contains("=" + variable + scopeSepPH) {

                            // Remove the "var " keyword from the variable declaration line
                            if results[i].hasPrefix(PH_RESV_KW_VAR) {
                                results[i] = String(results[i].dropFirst(PH_RESV_KW_VAR.count))
                            }

                            // Add an instantiation code at the top of our list:
                            // {className} {variable};
                            let splitNamespaceCode = rightExp.components(separatedBy: scopeSepPH)
                            let instanceObjName = splitNamespaceCode.joined(separator: ".")

                            // Find class name used in instantiation: {instanceObjName}=𒀭PH_NEW𒀭...
                            var className = ""

                            for k in 0..<codes.count {
                                if !codes[k].contains(instanceObjName) { continue }

                                // Search for 𒀭PH_NEW𒀭 until we reach a semicolon
                                var m = k
                                while m < codes.count {
                                    if codes[m].contains(PH_ID + PH_NEW + PH_ID) {
                                        // Grab class name between 𒀭PH_NEW𒀭 and "[" (from "𒀭PH_NEW𒀭N.INT32[")
                                        let newPH = NSRegularExpression.escapedPattern(for: PH_ID + PH_NEW + PH_ID)
                                        className = codes[m].firstMatchString("(?<=" + newPH + ").+?(?=\\[)")
                                        break
                                    }
                                    if codes[m].contains(";") {
                                        break
                                    }
                                    m += 1
                                }

                                if !className.isEmpty {
                                    break
                                }
                            }

                            // Replace dot separators with placeholders
                            className = className.replacingOccurrences(of: ".", with: scopeSepPH)

                            // Insert: "{className} {variable};"
                            results.insert(className + " " + variable + ";", at: 0)
                            break
                        }
                    }
                }
            }
        }

        // Resolve comma separators
        results = BreakDownCommaSeparators(results)

        // Resolve sequential function calls (functions that contain a period "." after itself)
        // NOTE: sequential function call separators were changed to "#" earlier.
        results = BreakDownFunctionCallSequence(results)

        // Convert math operations into RPN format
        results = ConvertExpressionToRPN(results)

        // Resolve new keyword instantiation line that contains "var " and replace the "var " with the actual class name type
        // In this stage, we only convert to pointer declaration + remove "var" keyword.
        var idx = 0
        while idx < results.count {
            let variable = results[idx].firstMatchString("(?<=" + NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) + ").+?(?==)")
            if !variable.isEmpty {
                if results[idx].contains(PH_ID + PH_NEW + PH_ID) {
                    results.insert(PH_RESV_KW_PTR + variable + ";", at: idx)
                    results[idx + 1] = results[idx + 1].replacingOccurrences(of: PH_RESV_KW_VAR, with: "")
                    idx += 1
                }
            }
            idx += 1
        }

        var finalResults = results.joined()

        // Replace placeholders back to original values
        finalResults = finalResults.replacingOccurrences(of: scopeSepPH, with: ".")

        // Convert back increment/decrement placeholders
        finalResults = finalResults.replacingOccurrences(of: incrementPH, with: "＋")
        finalResults = finalResults.replacingOccurrences(of: decrementPH, with: "—")

        // seqFuncCallPH is currently not used directly beyond generation; keep for parity/future.
        _ = seqFuncCallPH

        return finalResults
    }
    
    /// <summary>
    /// Break down expressions containing arrays, functions into smaller expression
    /// </summary>
    /// <param name="expressions"></param>
    /// <returns></returns>
    private static func BreakDownArrayFunc(_ expression: String) -> [String] {

        var combinedResults: [String] = []

        // If we have any arrays inside the expression, resolve them first
        var resultsArrayFuncResolved: [String] = []

        if expression.contains("[") {

            // Resolve arrays in expression
            // e.g. "N.a[N.b+1+c[6/add(2,3)]]=1+N.add(N.a[2],3)"
            resultsArrayFuncResolved =
                BreakDownExpressionWithBrackets(expression, "[", "]")

            // Remove redundant code and optimize it
            resultsArrayFuncResolved =
                EliminateRedundantExpressions(resultsArrayFuncResolved)

            // Next, resolve function calls (functions contain round brackets)
            for s in resultsArrayFuncResolved {

                if s.hasPrefix(PH_RESV_KW_VAR) {

                    // Are there any function calls?
                    if let _ = s.firstIndex(of: ")") {

                        let resolveExpTarget =
                            s
                            .dropFirst(PH_RESV_KW_VAR.count)
                            .replacingOccurrences(of: ";", with: "")

                        var funcResResults =
                            BreakDownExpressionWithBrackets(
                                String(resolveExpTarget),
                                "(",
                                ")"
                            )

                        // Restore "var " prefix to the last expression
                        if let last = funcResResults.last {
                            funcResResults[funcResResults.count - 1] =
                                PH_RESV_KW_VAR + last
                        }

                        combinedResults.append(contentsOf: funcResResults)

                    } else {
                        combinedResults.append(s)
                    }

                } else {

                    if let _ = s.firstIndex(of: ")") {

                        let resolveExpTarget =
                            s.replacingOccurrences(of: ";", with: "")

                        var funcResResults =
                            BreakDownExpressionWithBrackets(
                                resolveExpTarget,
                                "(",
                                ")"
                            )

                        // Remove redundant code and optimize it
                        funcResResults =
                            EliminateRedundantExpressions(funcResResults)

                        combinedResults.append(contentsOf: funcResResults)

                    } else {
                        combinedResults.append(s)
                    }
                }
            }

        } else {

            // Resolve function calls only
            // e.g. "c = add(2,sub(3,4))"
            combinedResults =
                BreakDownExpressionWithBrackets(expression, "(", ")")
        }

        return combinedResults
    }
    
    private static func BreakDownExpressionWithBrackets(
        _ expression: String,
        _ openingBracket: String,
        _ closingBracket: String
    ) -> [String] {

        var results: [String] = []

        // Before evaluating the expression, we need to see if the expression contains
        // an assignment operator:
        // "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>="
        // If so, we break down left and right expressions separately.
        let breakUpExpression = BreakUpAssignmentExpression(expression)

        // We found some kind of assignment operator
        if breakUpExpression[0] != nil {

            // Resolve left-hand side and right-hand side separately
            // NOTE: ResolveExpressionWithBrackets returns results in
            // "var xxx = yyyy;" format, so we only care about the final variable.
            let leftResults =
                ResolveExpressionWithBrackets(
                    breakUpExpression[1]!,
                    openingBracket,
                    closingBracket,
                    true
                )

            let rightResults =
                ResolveExpressionWithBrackets(
                    breakUpExpression[2]!,
                    openingBracket,
                    closingBracket
                )

            // Extract the final variable name from the RHS
            // Regex: (?<=var ).+?(?==)
            let rightFinal = rightResults.last ?? ""
            let rightResultFinalVar =
                rightFinal.firstMatchString(
                    "(?<=" +
                    NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) +
                    ").+?(?==)"
                )

            // Order matters: RHS first, then LHS with assignment applied
            results.append(contentsOf: rightResults)

            var modifiedLeftResults = leftResults
            if let lastIndex = modifiedLeftResults.indices.last {
                modifiedLeftResults[lastIndex] +=
                    breakUpExpression[0]! + rightResultFinalVar + ";"
            }

            results.append(contentsOf: modifiedLeftResults)

        } else {

            // No assignment operator, resolve expression directly
            results =
                ResolveExpressionWithBrackets(
                    expression,
                    openingBracket,
                    closingBracket
                )
        }

        return results
    }
    
    /// Breaks up expression into arrays in the following format:
    /// return [0] = assignment operator (e.g. "=", "-=", "+=", etc)
    /// return [1] = expression on the left of the assignment operator
    /// return [2] = expression on the right of the assignment operator
    ///
    /// - Parameter expression: Any expression that may or may not contain assignment operators
    /// - Returns: String array split by assignment operator
    private static func BreakUpAssignmentExpression(
        _ expression: String
    ) -> [String?] {

        // Return array:
        // [0] assignment operator
        // [1] left expression
        // [2] right expression
        var retStr: [String?] = [nil, nil, nil]

        let ignoreOperatorList = ["<=", ">=", "=="]
        let assignmentOperatorList = [
            "-=", "+=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "="
        ]

        var exp = expression

        // Replace logical operators containing "=" with placeholders
        for i in 0..<ignoreOperatorList.count {
            exp = exp.replacingOccurrences(
                of: ignoreOperatorList[i],
                with: PH_ID + String(i)
            )
        }

        // Search for assignment operators
        for op in assignmentOperatorList {

            if let range = exp.range(of: op) {

                let assignmentOperatorIndex =
                    exp.distance(from: exp.startIndex, to: range.lowerBound)

                // Found assignment operator
                retStr[0] = op
                retStr[1] = String(exp.prefix(assignmentOperatorIndex))
                retStr[2] = String(
                    exp.dropFirst(assignmentOperatorIndex + op.count)
                )

                // Convert compound assignment to basic assignment
                // e.g. x *= y - z  --->  x = x * (y - z)
                if let equalIndex = op.firstIndex(of: "="),
                   equalIndex != op.startIndex {

                    let operatorPart = String(op[..<equalIndex])
                    retStr[2] =
                        (retStr[1] ?? "")
                        + operatorPart
                        + "("
                        + (retStr[2] ?? "")
                        + ")"

                    retStr[0] = "="
                }

                break
            }
        }

        // No assignment operator found
        if retStr[0] == nil {
            return retStr
        }

        // Restore logical operators from placeholders
        for j in 0...2 {
            if var value = retStr[j] {
                for i in 0..<ignoreOperatorList.count {
                    value = value.replacingOccurrences(
                        of: PH_ID + String(i),
                        with: ignoreOperatorList[i]
                    )
                }
                retStr[j] = value
            }
        }

        return retStr
    }
    
    /// Resolves expressions containing brackets by breaking them down into
    /// temporary variable assignments.
    ///
    /// - Parameters:
    ///   - expression: Target expression string
    ///   - openingBracket: Opening bracket string, e.g. "(", "["
    ///   - closingBracket: Closing bracket string, e.g. ")", "]"
    ///   - isFinalStatementFloating: If true, returns only the final resolved expression
    /// - Returns: List of resolved expression statements
    private static func ResolveExpressionWithBrackets(
        _ expression: String?,
        _ openingBracket: String,
        _ closingBracket: String,
        _ isFinalStatementFloating: Bool = false
    ) -> [String] {

        var resultList: [String] = []
        guard var expr = expression else { return resultList }

        let openChar = Character(openingBracket)
        let closeChar = Character(closingBracket)

        var i = 0
        while i < expr.count {
            let idx = expr.index(expr.startIndex, offsetBy: i)
            let ch = expr[idx]

            if ch == openChar {
                i += 1
                continue
            }

            if ch == closeChar {

                let startFuncVarIndex =
                    ObtainFuncVarStartIndex(expr, i, openingBracket)

                let startIdx = expr.index(expr.startIndex, offsetBy: startFuncVarIndex)
                let endIdx = idx
                let fullArrayFuncStr =
                    String(expr[startIdx...endIdx])

                // Extract array / function name
                let nameOfArrayFunc =
                    fullArrayFuncStr.firstMatchString(#"^[^\(\[\{<]+"#)

                // Extract contents inside brackets
                let bracketContents =
                    fullArrayFuncStr.firstMatchString(
                        "(?<=" +
                        NSRegularExpression.escapedPattern(for: openingBracket) +
                        ").+?(?=" +
                        NSRegularExpression.escapedPattern(for: closingBracket) +
                        ")"
                    )

                let tmpVar =
                    RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

                var breakDownExp: String

                if !bracketContents.isEmpty {
                    breakDownExp =
                        PH_RESV_KW_VAR + tmpVar + "=" + bracketContents + ";"
                    resultList.append(breakDownExp)

                    breakDownExp =
                        PH_RESV_KW_VAR
                        + "result_"
                        + tmpVar
                        + "="
                        + nameOfArrayFunc
                        + openingBracket
                        + tmpVar
                        + closingBracket
                        + ";"
                } else {
                    breakDownExp =
                        PH_RESV_KW_VAR
                        + "result_"
                        + tmpVar
                        + "="
                        + nameOfArrayFunc
                        + openingBracket
                        + closingBracket
                        + ";"
                }

                resultList.append(breakDownExp)

                // Replace original expression with placeholder
                expr =
                    ReplaceFirstOccurance(
                        expr,
                        fullArrayFuncStr,
                        "result_" + tmpVar
                    )

                // Restart scan
                i = 0
                continue
            }

            i += 1
        }

        // No brackets found, wrap entire expression
        if resultList.isEmpty {
            let tmpVar =
                RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

            let breakDownExp =
                PH_RESV_KW_VAR + tmpVar + "=" + expr + ";"

            resultList.append(breakDownExp)
        }

        if isFinalStatementFloating {
            // Extract only RHS expression
            var finalExpression = resultList.last!
            finalExpression =
                finalExpression
                    .replacingOccurrences(
                        of: PH_RESV_KW_VAR + #"(.*?)="#,
                        with: "",
                        options: .regularExpression
                    )
                    .replacingOccurrences(of: ";", with: "")

            resultList[resultList.count - 1] = finalExpression
        } else {
            let finalVar =
                "final_"
                + RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

            let breakDownExp =
                PH_RESV_KW_VAR + finalVar + "=" + expr + ";"

            resultList.append(breakDownExp)
        }

        return resultList
    }
    
    /// Get the character index inside a string where the variable or function name starts
    /// e.g.
    /// 1+add(2,3)  --> Given endOfScopeSymbolIndex = 9 returns 2
    /// a-xb[2]     --> Given endOfScopeSymbolIndex = 6 returns 2
    ///
    /// - Parameters:
    ///   - expression: Target expression string
    ///   - endOfScopeSymbolIndex: Index of closing bracket ")" or "]"
    ///   - openBracket: Opening bracket "(" or "["
    ///   - isIgnorePeriod: Whether "." should be ignored when scanning identifier
    /// - Returns: Start index of variable or function name
    private static func ObtainFuncVarStartIndex(
        _ expression: String,
        _ endOfScopeSymbolIndex: Int,
        _ openBracket: String,
        _ isIgnorePeriod: Bool = true
    ) -> Int {

        let chars = Array(expression)

        guard endOfScopeSymbolIndex >= 0,
              endOfScopeSymbolIndex < chars.count else {
            return 0
        }

        let closeBracket = chars[endOfScopeSymbolIndex]
        let openBracketChar = Character(openBracket)

        var index = 0
        var bracketCounter = 0

        // --------------------------------------------------
        // Trace back to find matching opening bracket
        // --------------------------------------------------
        var j = endOfScopeSymbolIndex
        while j >= 0 {

            defer { j -= 1 }

            if chars[j] == closeBracket {
                bracketCounter += 1
                continue
            }

            if chars[j] == openBracketChar {
                bracketCounter -= 1

                if bracketCounter == 0 {
                    // --------------------------------------------------
                    // Trace back further to locate variable/function name
                    // --------------------------------------------------
                    var k = j - 1
                    while k >= 0 {

                        defer { k -= 1 }

                        let ch = String(chars[k])
                        let pattern =
                            "[a-zA-Z0-9_" +
                            (isIgnorePeriod ? "" : "\\.") +
                            NSRegularExpression.escapedPattern(for: PH_ID) +
                            "]"

                        if ch.range(of: pattern, options: .regularExpression) == nil {
                            index = k + 1
                            break
                        }
                    }
                    break
                }
            }
        }

        return index
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
    public static func EliminateRedundantExpressions(
        _ expressions: [String]
    ) -> [String] {

        var expressions = expressions

        // C# mirror:
        // Dictionary<string,int> declaredVarList = new Dictionary<string,int>();
        // Must preserve insertion order like C# Dictionary.ElementAt(j).
        var declaredKeys: [String] = []
        var declaredCounts: [String: Int] = [:]

        // --------------------------------------------------
        // ① Remove redundant outer brackets
        // e.g. var x=(y);  → var x=y;
        // --------------------------------------------------
        for i in 0..<expressions.count {
            let s = expressions[i]
            let testRight = s.firstMatchString("(?<==).+?(?=;)")

            // C#: (testRight.IndexOf("(") == 0) && (testRight.IndexOf(")") == testRight.Length - 1)
            if testRight.hasPrefix("("), testRight.hasSuffix(")") {
                let replaceWith = String(testRight.dropFirst().dropLast())
                expressions[i] =
                    s.replacingOccurrences(
                        of: "(?<==).+?(?=;)",
                        with: replaceWith,
                        options: .regularExpression
                    )
            }
        }

        // --------------------------------------------------
        // ② Analyze & optimize expressions
        // --------------------------------------------------
        for i in 0..<expressions.count {

            let varName =
                expressions[i].firstMatchString(
                    "(?<=" +
                    NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) +
                    ").+?(?==)"
                )

            if i + 1 < expressions.count {

                let chkVarCurrent = expressions[i]
                let chkVarNext = expressions[i + 1]
                let testRight = chkVarNext.firstMatchString("(?<==).+?(?=;)")

                if varName == testRight, !varName.isEmpty {
                    let optimizedExp =
                        chkVarNext.firstMatchString("(.*?)=")
                        + chkVarCurrent.firstMatchString("(?<==).+?;")

                    expressions[i] = ""
                    expressions[i + 1] = optimizedExp
                }
            }

            // C#: if(varName != "") declaredVarList.Add(varName, 0);
            if !varName.isEmpty {
                if declaredCounts[varName] == nil {
                    declaredKeys.append(varName)
                    declaredCounts[varName] = 0
                }
            }

            let expRight = expressions[i].firstMatchString("(?<==).+?(?=;)")

            // C#: if(expRight == "" && expressions[i].IndexOf(PH_RESV_KW_PTR) == -1) expressions[i] = "";
            if expRight.isEmpty && !expressions[i].contains(PH_RESV_KW_PTR) {
                expressions[i] = ""
            }

            // C#:
            // for each declared var:
            // if expression does not contain "var ", check whole expression; else check only RHS.
            for key in declaredKeys {

                if expressions[i].range(of: PH_RESV_KW_VAR) == nil {
                    if expressions[i].contains(key) {
                        declaredCounts[key, default: 0] += 1
                    }
                } else {
                    if expRight.contains(key) {
                        declaredCounts[key, default: 0] += 1
                    }
                }
            }
        }

        // --------------------------------------------------
        // ③ Remove redundant variables that are never used
        // C#: for (j...) if(item.Value == 0) expressions[j] = "";
        // --------------------------------------------------
        for j in 0..<declaredKeys.count {
            let key = declaredKeys[j]
            let count = declaredCounts[key] ?? 0
            if count == 0, j < expressions.count {
                expressions[j] = ""
            }
        }

        // Remove blanks
        expressions = expressions.filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }
        return expressions
    }
    
    /// If the expression contains a comma as separator, process them
    /// as separate variables from left to right in order.
    /// e.g.
    /// var mgs=2,3;
    /// result_mgs = add(mgs);
    /// --- converts to ---
    /// var xxx0=2;
    /// var xxx1=3;
    /// var xxx=add(xxx0, xxx1);
    /// result_mgs = xxx;
    public static func BreakDownCommaSeparators(
        _ uncleanedList: [String]
    ) -> [String] {

        var resultList = uncleanedList
        var i = 0

        while i < resultList.count {
            defer { i += 1 }

            // If this expression contains a comma
            if !resultList[i].contains(",") {
                continue
            }

            // Extract temp variable name declared by "var "
            let tmpVarName =
                resultList[i].firstMatchString(
                    "(?<=" +
                    NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) +
                    ").+?(?==)"
                )

            // If no "var " declaration exists, ignore
            if tmpVarName.isEmpty {
                continue
            }

            // Extract RHS and split by comma
            let rightExp =
                resultList[i].firstMatchString("(?<==).+?(?=;)")

            let splitParams =
                rightExp.split(separator: ",").map(String.init)

            let tmpVar =
                RandomString(
                    RND_ALPHABET_STRING_LEN_MAX,
                    true,
                    TmpVarIdentifier
                )

            var tmpList: [String] = []
            var newBracketContents = ""

            for j in 0..<splitParams.count {
                let breakDownExp =
                    PH_RESV_KW_VAR
                    + tmpVar
                    + String(j)
                    + "="
                    + splitParams[j]
                    + ";"

                tmpList.append(breakDownExp)

                newBracketContents +=
                    tmpVar
                    + String(j)
                    + (j == splitParams.count - 1 ? "" : ",")
            }

            // Remove original comma expression
            resultList.remove(at: i)

            // Replace "(tmpVarName)" in the next expression
            if i < resultList.count {
                resultList[i] =
                    resultList[i].replacingOccurrences(
                        of: "(" + tmpVarName + ")",
                        with: "(" + newBracketContents + ")"
                    )
            }

            // Insert generated temp assignments
            resultList.insert(contentsOf: tmpList, at: i)

            // Mirror C#: i += tmpList.Count;
            i += tmpList.count
        }

        return resultList
    }
    
    /// Break down / re-arrange sequential function calls
    /// e.g.
    /// var a = add(www,ggg);        // removed
    /// var b = ToString();         // removed
    /// var c = Replace(xxx,yyy);   // removed
    /// var final = a#b#c;          // replaced with add(www,ggg)#ToString()#Replace(xxx,yyy);
    private static func BreakDownFunctionCallSequence(
        _ expressions: [String]
    ) -> [String] {

        var expressions = expressions
        var i = 0

        while i < expressions.count {

            let s = expressions[i]

            // Only care about expressions containing "#"
            if s.contains("#") {

                // Left-hand side of assignment: "var xxx ="
                let operandLeftExpression =
                    s.firstMatchString("(.*?)=")

                let operandRightExpression =
                    String(s.dropFirst(operandLeftExpression.count))

                // Extract function-call variables split by "#"
                // Regex mirrors C#:
                // (?<=[^a-zA-Z_PHID])(.*?)(?=[^a-zA-Z0-9#_PHID])
                let funcCallStr =
                    operandRightExpression.firstMatchString(
                        "(?<=[^a-zA-Z_" +
                        NSRegularExpression.escapedPattern(for: PH_ID) +
                        "])(.*?)(?=[^a-zA-Z0-9#_" +
                        NSRegularExpression.escapedPattern(for: PH_ID) +
                        "])"
                    )

                let funcCalls = funcCallStr.split(separator: "#").map(String.init)

                var finalRightExpression = s
                var functionCallSequenceStr = ""

                // For each function call variable (a, b, c ...)
                for (j, callVar) in funcCalls.enumerated() {

                    // Search earlier expressions for:
                    // var a = actualFuncCall;
                    for k in 0..<i {

                        let declaredVar =
                            expressions[k].firstMatchString(
                                "(?<=" +
                                NSRegularExpression.escapedPattern(for: PH_RESV_KW_VAR) +
                                ").+?(?==)"
                            )

                        let actualFuncCall =
                            expressions[k].firstMatchString("(?<==).+?(?=;)")

                        if declaredVar == callVar {

                            // Replace variable with actual function call
                            finalRightExpression =
                                finalRightExpression.replacingOccurrences(
                                    of: callVar,
                                    with: actualFuncCall
                                )

                            functionCallSequenceStr +=
                                actualFuncCall +
                                (j < funcCalls.count - 1 ? "#" : "")

                            // This declaration becomes invalid
                            expressions[k] = ""
                            break
                        }
                    }
                }

                expressions[i] = finalRightExpression

                // If the sequential function call is mixed with operators,
                // extract it into a temporary variable
                // e.g.
                // str = str + add(...)#ToString()*x;
                let operatorPattern = #"([*()\^\/\+\-%&\|\^!~])"#

                if finalRightExpression.range(
                    of: operatorPattern,
                    options: .regularExpression
                ) != nil {

                    let tmpVar =
                        RandomString(
                            RND_ALPHABET_STRING_LEN_MAX,
                            true,
                            TmpVarIdentifier
                        )

                    expressions[i] =
                        expressions[i].replacingOccurrences(
                            of: functionCallSequenceStr,
                            with: tmpVar
                        )

                    expressions.insert(
                        PH_RESV_KW_VAR
                        + tmpVar
                        + "="
                        + functionCallSequenceStr
                        + ";",
                        at: i
                    )

                    i += 1
                }
            }

            i += 1
        }

        // Remove blank expressions
        expressions =
            expressions.filter {
                !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
            }

        return expressions
    }
    
    /// Converts simple math operations to RPN (Reverse Polish Notation) format
    private static func ConvertExpressionToRPN(
        _ expressions: [String]
    ) -> [String] {

        var expressions = expressions
        let ep = ExpressionParser()

        /*
         Operator precedence reference:

         -   12  UnaryMinus
         +   12  UnaryPlus
         !   12  UnaryNot
         ~   12  BitwiseUnaryComplement
         *   11  Multiply
         /   11  Divide
         %   11  Modulo
         +   10  Add
         -   10  Subtract

         <<  9   BitwiseShiftLeft
         >>  9   BitwiseShiftRight

         <   9   LessThan
         >   9   GreaterThan
         >=  9   GreaterThanOrEqual
         <=  9   LessThanOrEqual
         !=  8   NotEqual
         ==  8   Equality
         &   7   BitwiseAnd
         ^   6   BitwiseXor
         |   5   BitwiseOr
         &&  4   LogicalAnd
         ||  3   LogicalOr
         =   2   Assignment
         ,   1   Comma
         */

        for i in 0..<expressions.count {

            let expr = expressions[i]

            // Ignore function calls (foo(...))
            if expr.range(
                of: "[a-zA-Z0-9_" + NSRegularExpression.escapedPattern(for: PH_ID) + "]\\(",
                options: .regularExpression
            ) != nil {
                continue
            }

            // Ignore array access
            if expr.contains("[") {
                continue
            }

            // Ignore expressions without operators
            if expr.range(
                of: #"([*\^\/\+\-%&\|\^!~<>])"#,
                options: .regularExpression
            ) == nil,
               !expr.contains("==") {
                continue
            }

            // Split left-hand and right-hand expressions
            let operandLeftExpression =
                expr.firstMatchString("(.*?)=")

            var operandRightExpression =
                expr
                    .dropFirst(operandLeftExpression.count)
                    .replacingOccurrences(of: ";", with: "")

            // Temporary placeholders for increment/decrement
            let tmpVarInc =
                RandomString(
                    RND_ALPHABET_STRING_LEN_MAX,
                    true,
                    TmpVarIdentifier
                )

            let tmpVarDec =
                RandomString(
                    RND_ALPHABET_STRING_LEN_MAX,
                    true,
                    TmpVarIdentifier
                )

            operandRightExpression =
                operandRightExpression
                    .replacingOccurrences(of: "＋", with: tmpVarInc)
                    .replacingOccurrences(of: "—", with: tmpVarDec)

            // Parse expression into RPN token list
            let compiledExpression = ep.Parse(String(operandRightExpression))

            // Build RPN string
            var rpnStr = "\""
            for token in compiledExpression {
                rpnStr += token.ToString() + "#"
            }

            rpnStr.removeLast() // remove trailing "#"
            rpnStr += "\";"

            expressions[i] = operandLeftExpression + rpnStr

            // Restore increment/decrement symbols
            expressions[i] =
                expressions[i]
                    .replacingOccurrences(of: tmpVarInc, with: "＋")
                    .replacingOccurrences(of: tmpVarDec, with: "—")
        }

        return expressions
    }
    
    /// <summary>
    /// For all "var" keywords, determine their appropriate size if possible - if not able to determine, it will be converted to the most largest variable type (long)
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ResolveImplicitVarType(_ codes: [String]) -> [String] {

        var codes = codes

        // Get all variables inside the code that are assigned with values
        let allValueAssignedVariableList =
            GetAllValueAssignedVariablesInCode(codes)

        var i = 0
        while i < codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count <= 1 {
                i += 1
                continue
            }

            if splitCode[0] + " " == PH_RESV_KW_VAR {

                var varToCheck = splitCode[1]
                let listOfVarAssignment =
                    allValueAssignedVariableList[varToCheck]

                // --------------------------------------------------
                // No assignment → LONG
                // --------------------------------------------------
                if listOfVarAssignment == nil || listOfVarAssignment!.isEmpty {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            PH_RESV_KW_VAR,
                            PH_RESV_KW_LONG
                        )
                    i += 1
                    continue
                }

                // --------------------------------------------------
                // TMP variable → always LONG
                // --------------------------------------------------
                if varToCheck.contains(TmpVarIdentifier) {

                    if i + 1 < codes.count, codes[i + 1] != ";" {
                        varToCheck = ";" + varToCheck
                    } else {
                        varToCheck = ""
                    }

                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            PH_RESV_KW_VAR,
                            PH_RESV_KW_LONG
                        ) + varToCheck

                    i += 1
                    continue
                }

                // --------------------------------------------------
                // String placeholder → PTR
                // --------------------------------------------------
                if let assignments = listOfVarAssignment {
                    for value in assignments {
                        if value.contains(PH_ID + PH_STR) {

                            ActualClassMemberPointerVarTypeList[varToCheck] =
                                PH_RESV_KW_CHAR.trimmingCharacters(in: .whitespaces)

                            if i + 1 < codes.count, codes[i + 1] != ";" {
                                varToCheck = ";" + varToCheck
                            } else {
                                varToCheck = ""
                            }

                            codes[i] =
                                ReplaceFirstOccurance(
                                    codes[i],
                                    PH_RESV_KW_VAR,
                                    PH_RESV_KW_PTR
                                ) + varToCheck

                            i += 1
                            continue
                        }
                    }
                }

                // --------------------------------------------------
                // "new" keyword → ULONG
                // --------------------------------------------------
                var isNewKeyword = false
                if let assignments = listOfVarAssignment {
                    for value in assignments {
                        if value.contains(PH_ID + PH_NEW + PH_ID) {
                            isNewKeyword = true
                            break
                        }
                    }
                }

                if isNewKeyword {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            PH_RESV_KW_VAR,
                            PH_RESV_KW_ULONG
                        )

                    if i + 1 < codes.count, codes[i + 1].contains("=") {
                        codes[i] =
                            ReplaceFirstOccurance(
                                codes[i],
                                PH_RESV_KW_ULONG,
                                PH_RESV_KW_ULONG + varToCheck + ";"
                            )
                    }

                    i += 1
                    continue
                }

                // --------------------------------------------------
                // Decimal detection → DOUBLE
                // --------------------------------------------------
                var isDecimalValue = false
                if let assignments = listOfVarAssignment {
                    for value in assignments {
                        if value.contains(".") {
                            isDecimalValue = true
                            if value.contains("(") || value.contains("[") {
                                isDecimalValue = false
                                break
                            }
                        }
                    }
                }

                if isDecimalValue {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            PH_RESV_KW_VAR,
                            PH_RESV_KW_DOUBLE
                        )

                    if i + 1 < codes.count, codes[i + 1].contains("=") {
                        codes[i] =
                            ReplaceFirstOccurance(
                                codes[i],
                                PH_RESV_KW_DOUBLE,
                                PH_RESV_KW_DOUBLE + varToCheck + ";"
                            )
                    }

                    i += 1
                    continue
                }

                // --------------------------------------------------
                // Numeric check
                // --------------------------------------------------
                var isAllNumeric = true
                if let assignments = listOfVarAssignment {
                    for value in assignments {
                        if Int64(value) == nil {
                            isAllNumeric = false
                            break
                        }
                    }
                }

                if !isAllNumeric {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            PH_RESV_KW_VAR,
                            PH_RESV_KW_LONG
                        )

                    if i + 1 < codes.count, codes[i + 1].contains("=") {
                        codes[i] =
                            ReplaceFirstOccurance(
                                codes[i],
                                PH_RESV_KW_LONG,
                                PH_RESV_KW_LONG + varToCheck + ";"
                            )
                    }

                    i += 1
                    continue
                }

                // --------------------------------------------------
                // Range-based type determination
                // --------------------------------------------------
                let numericValues =
                    listOfVarAssignment!
                        .compactMap { Int64($0) }
                        .sorted()

                let minVal = numericValues.first!
                let maxVal = numericValues.last!

                var determinedVarType = PH_RESV_KW_LONG

                if TARGET_ARCH_BIT_SIZE == 16 {
                    if minVal >= 0 && maxVal <= 255 {
                        determinedVarType = PH_RESV_KW_BYTE
                    } else if minVal >= -128 && maxVal <= 127 {
                        determinedVarType = PH_RESV_KW_SBYTE
                    } else if minVal >= 0 && maxVal <= 65535 {
                        determinedVarType = PH_RESV_KW_USHORT
                    } else if minVal >= -32768 && maxVal <= 32767 {
                        determinedVarType = PH_RESV_KW_SHORT
                    }
                } else if TARGET_ARCH_BIT_SIZE == 32 {
                    if minVal >= 0 && maxVal <= 255 {
                        determinedVarType = PH_RESV_KW_BYTE
                    } else if minVal >= -128 && maxVal <= 127 {
                        determinedVarType = PH_RESV_KW_SBYTE
                    } else if minVal >= 0 && maxVal <= 65535 {
                        determinedVarType = PH_RESV_KW_USHORT
                    } else if minVal >= -32768 && maxVal <= 32767 {
                        determinedVarType = PH_RESV_KW_SHORT
                    } else if minVal >= 0 && maxVal <= 4294967295 {
                        determinedVarType = PH_RESV_KW_UINT
                    } else if minVal >= -2147483648 && maxVal <= 2147483647 {
                        determinedVarType = PH_RESV_KW_INT
                    }
                } else if TARGET_ARCH_BIT_SIZE == 64 {
                    if minVal >= 0 && maxVal <= 255 {
                        determinedVarType = PH_RESV_KW_BYTE
                    } else if minVal >= -128 && maxVal <= 127 {
                        determinedVarType = PH_RESV_KW_SBYTE
                    } else if minVal >= 0 && maxVal <= 65535 {
                        determinedVarType = PH_RESV_KW_USHORT
                    } else if minVal >= -32768 && maxVal <= 32767 {
                        determinedVarType = PH_RESV_KW_SHORT
                    } else if minVal >= 0 && maxVal <= 4294967295 {
                        determinedVarType = PH_RESV_KW_UINT
                    } else if minVal >= -2147483648 && maxVal <= 2147483647 {
                        determinedVarType = PH_RESV_KW_INT
                    } else if minVal >= -9223372036854775808 && maxVal <= 9223372036854775807 {
                        determinedVarType = PH_RESV_KW_LONG
                    } else if minVal >= 0 && maxVal > 9223372036854775807 {
                        determinedVarType = PH_RESV_KW_ULONG
                    }
                }

                codes[i] =
                    ReplaceFirstOccurance(
                        codes[i],
                        PH_RESV_KW_VAR,
                        determinedVarType
                    )

                if i + 1 < codes.count, codes[i + 1].contains("=") {
                    codes[i] =
                        ReplaceFirstOccurance(
                            codes[i],
                            determinedVarType,
                            determinedVarType + varToCheck + ";"
                        )
                }
            }

            i += 1
        }

        // Clean again
        codes = RegenerateCleanCode(codes)
        return codes
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
    /// <returns>Dictionary of (varName, [list of RHS expressions])</returns>
    private static func GetAllValueAssignedVariablesInCode(
        _ codes: [String]
    ) -> [String: [String]] {

        var allValueAssignedVariableList: [String: [String]] = [:]

        for i in 0..<codes.count {

            // For all codes, search for the assignment symbol
            if codes[i] != "=" {
                continue
            }

            // Safety guard (C# implicitly assumes valid range)
            if i - 1 < 0 || i + 1 >= codes.count {
                continue
            }

            // Check if not a logical operator (<=, >=, !=, ==)
            // C#: !Regex.IsMatch(codes[i - 1], @"[<|>|!|=]") && codes[i + 1] != "="
            if codes[i - 1].range(
                of: "[<|>|!|=]",
                options: .regularExpression
            ) != nil {
                continue
            }

            if codes[i + 1] == "=" {
                continue
            }

            // --------------------------------------------------
            // Resolve left-hand variable name
            // --------------------------------------------------
            var leftOfOperator = codes[i - 1]
            let splitCode = leftOfOperator.split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            if splitCode.count > 1 {
                // e.g. "var x" → "x", "int y" → "y"
                leftOfOperator = splitCode[1]
            } else {
                leftOfOperator = splitCode[0]
            }

            if allValueAssignedVariableList[leftOfOperator] == nil {
                allValueAssignedVariableList[leftOfOperator] = []
            }

            // --------------------------------------------------
            // Get full expression from assignment operator
            // --------------------------------------------------
            var mutableCodes = codes
            let expression =
                GetFullExpressionStatementFromAssignmentOperator(
                    &mutableCodes,
                    i
                ) + ";"

            // Extract RHS expression
            let rightOfOperator =
                expression.firstMatchString("(?<==).+?(?=;)")

            allValueAssignedVariableList[leftOfOperator]?.append(rightOfOperator)
        }

        return allValueAssignedVariableList
    }
    
    /// <summary>
    /// Given a token separated line of codes, this method returns the assignment operation statement,
    /// given the index where the assignment operator is found in the code array
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="assignmentOperatorIndex"></param>
    /// <param name="isRemoveExpressionFromCode">
    /// Determines whether the extracted expression tokens should be removed from codes
    /// </param>
    /// <returns></returns>
    private static func GetFullExpressionStatementFromAssignmentOperator(
        _ codes: inout [String],
        _ assignmentOperatorIndex: Int,
        _ isRemoveExpressionFromCode: Bool = false
    ) -> String {

        var expEndIndex = -1
        var expStartIndex = -1

        // --------------------------------------------------
        // Find end of expression (until ;)
        // --------------------------------------------------
        var j = assignmentOperatorIndex
        while j < codes.count {
            if codes[j] == ";" {
                expEndIndex = j - 1
                break
            }
            j += 1
        }

        // --------------------------------------------------
        // Find start of expression (after ; ( ) { })
        // --------------------------------------------------
        j = assignmentOperatorIndex
        while j >= 0 {
            if codes[j].range(
                of: #"[;|\(|\)|\{|\}]"#,
                options: .regularExpression
            ) != nil {
                expStartIndex = j + 1
                break
            }
            j -= 1
        }

        // --------------------------------------------------
        // Build expression string
        // --------------------------------------------------
        var expression = ""

        if expStartIndex >= 0, expEndIndex >= expStartIndex {
            for idx in expStartIndex...expEndIndex {
                expression += codes[idx]
                if isRemoveExpressionFromCode {
                    codes[idx] = ""
                }
            }
        }

        return expression
    }
    
    /// <summary>
    /// For each class, move all member variable initiation codes to all declared constructor(s),
    /// and variable assignments to top of class.
    /// Prerequisite: At least one constructor must be created beforehand
    /// </summary>
    private static func RestructureClassMemberVars(
        _ inputCodes: [String],
        _ keyword: String
    ) -> [String] {

        var codes = inputCodes

        // For each class or static class
        for index in GenerateKeywordIndexList(codes, keyword) {

            // ---------------------------------------------
            // Extract class name (supports namespace)
            // ---------------------------------------------
            let classWithNamespaceName =
                ReplaceFirstOccurance(codes[index], keyword, "")

            let className =
                classWithNamespaceName
                    .split(separator: ".")
                    .map(String.init)
                    .last ?? ""

            // ---------------------------------------------
            // Find class scope
            // ---------------------------------------------
            let endClassScopeBlock =
                FindEndScopeBlock(codes, index, "{", "}")

            if endClassScopeBlock == -1 {
                continue
            }

            // Find where class member declarations should be inserted
            var classMemberDeclarationInsertionIndex = -1
            var i = index + 1
            while i < endClassScopeBlock {
                if codes[i] == "{" {
                    classMemberDeclarationInsertionIndex = i
                    break
                }
                i += 1
            }

            if classMemberDeclarationInsertionIndex == -1 {
                continue
            }

            // ---------------------------------------------
            // Collect constructor insertion points
            // ---------------------------------------------
            var constructorMemberInsertionIndexes: [Int] = []

            var declarationCode = ""
            var initiationCode = ""

            i = classMemberDeclarationInsertionIndex + 1
            while i < endClassScopeBlock {

                let splitFuncName =
                    codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)

                // -----------------------------------------
                // Function / Constructor detection
                // -----------------------------------------
                if !splitFuncName.isEmpty,
                   AllFunctionKeywords.contains(splitFuncName[0] + " ") {

                    let endFunctionParamBlock =
                        FindEndScopeBlock(codes, i, "(", ")")

                    // Constructor = function name == class name
                    if codes[i] ==
                        splitFuncName[0]
                        + " "
                        + classWithNamespaceName
                        + "."
                        + className {

                        constructorMemberInsertionIndexes.append(
                            endFunctionParamBlock + 1
                        )
                    }

                    let endFunctionScopeBlock =
                        FindEndScopeBlock(
                            codes,
                            endFunctionParamBlock,
                            "{",
                            "}"
                        )

                    i = endFunctionScopeBlock + 1
                    continue
                }

                // -----------------------------------------
                // Ignore labels
                // -----------------------------------------
                if !splitFuncName.isEmpty,
                   splitFuncName[0].contains(PH_ID + PH_LABEL + PH_ID) {
                    i += 1
                    continue
                }

                // -----------------------------------------
                // Variable declaration (no "=")
                // -----------------------------------------
                if i + 1 < endClassScopeBlock, codes[i + 1] != "=" {

                    if codes[i].contains(" ") {
                        var j = i
                        while j < endClassScopeBlock {

                            declarationCode += codes[j]

                            if codes[j] == ";" {
                                codes[j] = ""
                                i = j + 1
                                break
                            }

                            codes[j] = ""
                            j += 1
                        }
                    } else {
                        i += 1
                    }

                    continue
                }

                // -----------------------------------------
                // Variable assignment → initiation code
                // -----------------------------------------
                var j = i
                while j < endClassScopeBlock {

                    initiationCode += codes[j]

                    if codes[j] == ";" {
                        codes[j] = ""
                        i = j + 1
                        break
                    }

                    codes[j] = ""
                    j += 1
                }
            }

            // ---------------------------------------------
            // Insert initiation code into constructors
            // ---------------------------------------------
            for idx in constructorMemberInsertionIndexes {
                if idx >= 0 && idx < codes.count {
                    codes[idx] += " " + initiationCode
                }
            }

            // ---------------------------------------------
            // Insert declarations at top of class
            // ---------------------------------------------
            if classMemberDeclarationInsertionIndex >= 0,
               classMemberDeclarationInsertionIndex < codes.count {
                codes[classMemberDeclarationInsertionIndex] +=
                    " " + declarationCode
            }
        }

        // Clean & re-tokenize
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Deploy all manually coded IASM codes in the script
    /// Typically, after several pre-processing, the IASM code will have the following structure by now:
    /// __________________________________
    /// int yyy;
    /// long xxx;
    /// xxx = 𒀭PH_STR0𒀭;
    /// yyy = DSystem.IASM.__iasm(xxx);
    /// __________________________________
    ///
    /// Therefore, we will remove the entire code above and replace it with the actual value of 𒀭PH_STRø𒀭
    /// (Where ø is the string list ID)
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func DeployManualIASM(_ codes: [String]) -> [String] {
        var codes = codes

        var i = 0
        while i < codes.count {
            // Look for IASM function call token
            if codes[i] != PH_RESV_KW_IASM_FUNCTION_CALL {
                i += 1
                continue
            }

            // --------------------------------------------
            // Example pattern at this point:
            //
            // int yyy
            // ;
            // long xxx
            // ;
            // xxx
            // =
            // 𒀭PH_STR0𒀭
            // ;
            // yyy
            // =
            // DSystem.IASM.__iasm
            // (
            // xxx
            // )
            // ;
            // --------------------------------------------

            // Dummy variable that receives the IASM call return value
            // yyy = __iasm(xxx);
            let dummyPopVariableOfCall = codes[i - 2]

            // Find end index of this statement (;)
            var endIndex = -1
            var j = i
            while j < codes.count {
                if codes[j] == ";" {
                    endIndex = j
                    break
                }
                j += 1
            }

            if endIndex == -1 {
                i += 1
                continue
            }

            // We search backward to find:
            // - placeholder string (𒀭PH_STRx𒀭)
            // - beginning of declaration (int yyy)
            let startCodeToSearch = PH_RESV_KW_INT + dummyPopVariableOfCall

            var startIndex = -1
            var placeHolderString = ""

            j = i
            while j >= 0 {
                if codes[j].contains(PH_ID + PH_STR) {
                    placeHolderString = codes[j]
                }
                if codes[j] == startCodeToSearch {
                    startIndex = j
                    break
                }
                j -= 1
            }

            if startIndex == -1 || placeHolderString.isEmpty {
                i += 1
                continue
            }

            // Remove all codes from startIndex to endIndex
            for k in startIndex...endIndex {
                codes[k] = ""
            }

            // Replace with actual IASM code (remove quotes)
            if let asm = StringLiteralList[placeHolderString] {
                codes[startIndex] = asm.replacingOccurrences(of: "\"", with: "") + ";"
            }

            i = endIndex + 1
        }

        return codes
    }
    
    /// <summary>
    /// Convert all string placeholders to char arrays assigned with actual decimal (UTF-16 / Decimal) representation
    /// IMPORTANT: Since we are allocating dynamic memory in the virtual heap, we must release the memory allocated after it has been used!
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertStringPlaceHolderToCharDecimalArray(_ codes: [String]) -> [String] {
        // Instance object list (varName -> type)
        let instanceObjList = GetInstanceObjects(codes)
        var codeList = codes

        var i = 0
        while i < codeList.count {
            // Look for string placeholder
            if !codeList[i].contains(PH_ID + PH_STR) {
                i += 1
                continue
            }

            // Must be assignment: varName = PH_STRx
            if i < 2 || codeList[i - 1] != "=" {
                i += 1
                continue
            }

            let varName = codeList[i - 2]

            // Fetch actual string literal
            guard var str = StringLiteralList[codeList[i]] else {
                i += 1
                continue
            }

            // Remove surrounding quotes
            if str.count >= 2 {
                str = String(str.dropFirst().dropLast())
            }

            // Unescape sequences (mirror C# logic)
            str = str.replacingOccurrences(of: "\\\\", with: "\\")
            str = str.replacingOccurrences(of: "\\n", with: "\n")
            str = str.replacingOccurrences(of: "\\t", with: "\t")
            str = str.replacingOccurrences(of: "\\0", with: "\0")

            // Append null terminator
            str.append("\0")

            // Determine if we need to instantiate the char array
            var isInstantiateObj = false
            if instanceObjList[varName] == nil {
                isInstantiateObj = true
            } else if instanceObjList[varName]! + " " == PH_RESV_KW_PTR {
                isInstantiateObj = true
            }

            // Instantiate: varName = new char[n]
            if isInstantiateObj {
                codeList[i - 2] = varName
                codeList[i - 1] = "="
                codeList[i] =
                    PH_ID + PH_NEW + PH_ID +
                    PH_RESV_KW_CHAR.trimmingCharacters(in: .whitespaces) +
                    "[\(str.count)]"
            }

            // Insert char assignments (reverse order, same as C#)
            var arrayIndex = str.count - 1
            for ch in str.reversed() {
                let c = UInt16(ch.unicodeScalars.first!.value)
                codeList.insert(";", at: i + 2)
                codeList.insert(String(c), at: i + 2)
                codeList.insert("=", at: i + 2)
                codeList.insert("\(varName)[\(arrayIndex)]", at: i + 2)
                arrayIndex -= 1
            }

            i += 1
        }

        return codeList
    }
    
    /// <summary>
    /// Move any variables that are declared as class members to constructor,
    /// if they are not being used in other places other than within the constructor
    /// </summary>
    private static func MoveTemporaryClassMemberVarToConstructor(
        _ inputCodes: [String],
        _ keyword: String
    ) -> [String] {

        var codes = inputCodes

        // For each class / static class
        for index in GenerateKeywordIndexList(codes, keyword) {

            let classWithNameSpaceName =
                ReplaceFirstOccurance(codes[index], keyword, "")
            let classNameSplit = classWithNameSpaceName.split(separator: ".").map(String.init)
            let className = classNameSplit.last ?? ""

            let constructor =
                PH_RESV_KW_FUNCTION_VOID
                + classWithNameSpaceName
                + "."
                + className

            var classMemberDeclarationInsertionIndex = -1
            var constructorIndex = -1

            // Find end of class scope
            let endClassScopeBlock =
                FindEndScopeBlock(codes, index, "{", "}")

            if endClassScopeBlock == -1 { continue }

            // Find class member declaration start + constructor index
            var i = index + 1
            while i < endClassScopeBlock {
                if codes[i] == "{" {
                    classMemberDeclarationInsertionIndex = i
                }
                if codes[i] == constructor {
                    constructorIndex = i
                    break
                }
                i += 1
            }

            // No constructor → skip
            if constructorIndex == -1 { continue }

            // Find constructor body insertion index
            var constructorMemberVarInsertionIndex = -1
            i = constructorIndex
            while i < endClassScopeBlock {
                if codes[i] == "{" {
                    constructorMemberVarInsertionIndex = i
                    break
                }
                i += 1
            }

            if constructorMemberVarInsertionIndex == -1 { continue }

            // Collect declared member variables
            var declaredMemberVarList: [String: Int] = [:]
            var firstFunctionFoundIndex = endClassScopeBlock

            i = classMemberDeclarationInsertionIndex + 1
            while i < endClassScopeBlock {

                let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                if splitCode.count < 2 {
                    i += 1
                    continue
                }

                // Stop when first function is found
                if AllFunctionKeywords.contains(splitCode[0] + " ") {
                    firstFunctionFoundIndex = i
                    break
                }

                // Ignore labels
                if splitCode[0].contains(PH_ID + PH_LABEL + PH_ID) {
                    i += 1
                    continue
                }

                // Store member variable (name -> index)
                declaredMemberVarList[splitCode[1]] = i
                i += 1
            }

            // Remove temporary member variables (TmpVarIdentifier) from class top
            // and move them into constructor
            var declarationCode = ""

            for (varName, declIndex) in declaredMemberVarList {
                if !varName.contains(TmpVarIdentifier) {
                    continue
                }

                // Append declaration
                declarationCode += codes[declIndex] + ";"

                // Remove original declaration
                codes[declIndex] = ""
            }

            // Insert declarations at beginning of constructor body
            if !declarationCode.isEmpty {
                codes[constructorMemberVarInsertionIndex] += " " + declarationCode
            }
        }

        // Re-tokenize
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    // -------------------------------------------------------
    // CONVERT ITERMEDIATE CODE TO VIRTUAL ASSEMBLY LANGUAGE
    // -------------------------------------------------------
    
    /// <summary>
    /// Converts preprocessed intermediatary source code to Assembly language codes
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ConvertIntermCodeToAsm(
        _ codes: [String],
        _ co: CompileOptions
    ) -> [String] {

        var codes = codes

        // Get all of the declared static class constructor list
        let declaredStaticClassConstructorList =
            GetDeclaredStaticClassConstructorList(codes)
        // Get all of the declared variable list (declared variable name, type), where type = "{u|s}BitSize" (e.g. u64, s32)
        var declaredVarList =
            GetDeclaredVarList(codes)

        // If we need to write intermediate code to file, we do so here
        let intermScriptFileName =
            URL(fileURLWithPath: co.FileName)
                .deletingPathExtension()
                .lastPathComponent + ".interm"

        let intermScriptOutputFileName =
            URL(fileURLWithPath: co.FileName)
                .deletingLastPathComponent()
                .appendingPathComponent(intermScriptFileName)
                .path

        if co.IsOutputIntermCode {
            try? codes.joined(separator: "\n")
                .write(
                    toFile: intermScriptOutputFileName,
                    atomically: true,
                    encoding: .utf8
                )
        }


        // Get all the instance object list (instance obj name, class name)
        let instanceObjList =
            GetInstanceObjects(codes)
        
        // Get all the function list (function name, type)
        let declaredFuncList =
            GetDeclaredFunctionList(codes)

        // Remove namespace and scope brackets as we do not need them anymore
        ShowLog("- Removing namespaces...")
        codes = RemoveNamespaces(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestRemoveNamespaces = codes }
        
        // Convert class member variables to pointer declarations/pointer vars
        ShowLog("- Converting class member vars to pointers...")
        codes = PrepareClassStructure(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestPrepareClassStructure = codes }
        
        // Convert all variable declarations to ASM code
        ShowLog("- Converting var declarations to iASM code...")
        codes = ConvertVarDeclarationsToASM(codes, declaredVarList)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertVarDeclarationsToASM = codes }
        
        // Convert increment statements on its own to one line
        ShowLog("- Resolving increment statements...")
        codes = ConvertIncDecToOneLine(codes, "+")
        
        // Convert decrement statements on its own to one line
        ShowLog("- Resolving decrement statements...")
        codes = ConvertIncDecToOneLine(codes, "-")
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertIncDecToOneLine = codes }
        
        // Resolve stand-alone increment/decrement codes
        ShowLog("- Resolving stand-alone increment/decrement codes...")
        codes = ResolveStandAloneIncrementDecrement(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestResolveStandAloneIncrementDecrement = codes }
        
        
        // Convert mathmatical expressions to ASM code
        ShowLog("- Converting expressions to iASM code...")
        codes = ConvertExpressionsToASM(codes, declaredVarList, instanceObjList, declaredFuncList)

        // Test
        if IsGenerateUnitTestFiles { TestConvertExpressionsToASM = codes }
        
        // Converts variables that are class instances to pointer format
        ShowLog("- Converting class instances to pointers...")
        codes = ConvertClassVarDeclarationsToPointers(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertClassVarDeclarationsToPointers = codes }
        
        // Convert return statement to "RET" ASM code
        ShowLog("- Converting return statements to iASM code...")
        codes = ConvertReturnToASM(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertReturnToASM = codes }
        
        // Converts instance's function calls and direct member variable access codes to ASM code
        ShowLog("- Converting instance function calls and direct member access to iASM code...")
        codes = ConvertInstanceFuncCallAndMemberVarAccessToASM(codes, instanceObjList, declaredVarList)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertInstanceFuncCallAndMemberVarAccessToASM = codes }
        
        // Convert if statements to ASM code
        ShowLog("- Converting if statements to iASM code...")
        codes = ConvertIfStatementToASM(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertIfStatementToASM = codes }
        
        // Convert function parameters to ASM code
        ShowLog("- Converting function params to iASM code...")
        codes = ConvertFunctionParamsToASM(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertFunctionParamsToASM = codes }
        
        // Replace other codes to ASM code
        ShowLog("- Replacing keywords to iASM code...")
        codes = ReplaceKeywordsToASM(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestReplaceKeywordsToASM = codes }
        
        // Convert class/functions to labels
        ShowLog("- Converting class/functions to labels...")
        codes = ConvertClassFunctionsToLabels(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertClassFunctionsToLabels = codes }
        
        // Let's clean the codes again by separating into tokens
        ShowLog("- Regenerating clean code...")
        codes = RegenerateCleanCode(codes)
        
        // Generate clean ASM code list
        ShowLog("- Generating clean iASM code...")
        codes = GenerateCleanASMCode(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestGenerateCleanASMCode = codes }
        
        
        // Convert pointer identifier (PH_ID + PH_POINTER + PH_ID) to "*" symbol
        ShowLog("- Replacing pointer identifiers...")
        codes = ConvertPointerKeywordsToSymbol(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertPointerKeywordsToSymbol = codes }
        
        // Adds VM specific codes (like preserved variables)
        ShowLog("- Adding VM specific codes (preserved vars)...")
        codes = AddVMSpecificCode(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestAddVMSpecificCode = codes }
        
        // Optimizes ASM code to remove redundant codes
        ShowLog("- Removing redundant iASM codes...")
        codes = OptimizeRedundantASM(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestOptimizeRedundantASM = codes }
        
        // --- REG / VAR handling ---
        let allREGKeywords = GenerateRegVarKeywords(PH_REG)
        let allVarKeywords = GenerateRegVarKeywords(PH_VAR)
        let registeredVarList = GetRegisteredVarList(codes, allREGKeywords)

        // Move all REG operations to top of code, and create a "main" label for the rest of code
        ShowLog("- Moving all REG operations to top of code/generating main label...")
        codes = MoveREGVarAndVarInitiationToTopOfCode(codes, allREGKeywords, registeredVarList, declaredStaticClassConstructorList)
        
        // Test
        if IsGenerateUnitTestFiles { TestMoveREGVarAndVarInitiationToTopOfCode = codes }
        
        // Hence we won't need to distinguish between REG and VAR
        ShowLog("- Converting REG keywords to VAR keyword...")
        codes = ConvertRegToVarCommand(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertRegToVarCommand = codes }
        
        // For exporting iasm simulator debugging code: Remove all labels and convert label references on JMP and JMS to absolute program code index
        ShowLog("- Removing labels and converting JMP/JMS instruction to jump to code index...");
        var iasmCodes = codes
        iasmCodes = RemoveLabelsAndConvertJumpToCodeIndex(iasmCodes)

        if co.IsOutputIntermCode {
            let asmFileName =
                URL(fileURLWithPath: co.FileName)
                    .deletingPathExtension()
                    .lastPathComponent + ".iasm"

            let asmOutputPath =
                URL(fileURLWithPath: co.FileName)
                    .deletingLastPathComponent()
                    .appendingPathComponent(asmFileName)
                    .path

            try? iasmCodes.joined(separator: "\n")
                .write(
                    toFile: asmOutputPath,
                    atomically: true,
                    encoding: .utf8
                )
        }
        
        // Test
        if IsGenerateUnitTestFiles { TestRemoveLabelsAndConvertJumpToCodeIndex = iasmCodes }
        
        // Remove any blank array list items
        ShowLog("- Removing blank items...")
        codes = codes.filter { !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty }
        
        // Add extra line to the code for the @END label
        codes.append("") // @END label space
        
        // Convert all variable names that are declared with REG and VAR to numeric indexes with preceeding "$" symbol
        ShowLog("- Converting variables to IDs...")
        codes = ConvertVariablesToID(codes, registeredVarList)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertVariablesToID = codes }
        
        // Convert all labels to numeric IDs
        ShowLog("- Converting all labels to numeric IDs...")
        codes = ConvertJumpAndLabelsToNumericID(codes)
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertJumpAndLabelsToNumericID = codes }
        
        return codes
    }
    
    /// <summary>
    /// Gets the entire static classe's constructor list that are declared in the code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func GetDeclaredStaticClassConstructorList(
        _ codes: [String]
    ) -> [String] {

        var declaredStaticClassList: [String] = []

        for code in codes {

            // Split by space (C# Split(' '))
            let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            // Must be exactly: "static class X.Y"
            if splitCode.count != 2 {
                continue
            }

            // Check static class keyword
            if splitCode[0] + " " != PH_RESV_KW_STATIC_CLASS {
                continue
            }

            // Split class name by namespace
            let splitClassName = splitCode[1].split(separator: ".").map(String.init)

            // Last component is the class name itself
            guard let classNameWithoutScope = splitClassName.last else {
                continue
            }

            // Add: FullName.ClassName
            // e.g. Foo.Bar -> Foo.Bar.Bar
            declaredStaticClassList.append(
                splitCode[1] + "." + classNameWithoutScope
            )
        }

        return declaredStaticClassList
    }
    
    /// <summary>
    /// Gets the entire variable list that are declared in the code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func GetDeclaredVarList(
        _ codes: [String]
    ) -> [String: String] {

        // --------------------------------------------------
        // Get all class names
        // --------------------------------------------------
        var declarationKeywordList =
            GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false)

        // Append space (mirror C# behavior)
        declarationKeywordList =
            declarationKeywordList.map { $0 + " " }

        let listClassNames = declarationKeywordList

        // Class member keywords + class names
        var listClassMemberKeywords = ClassMemberReservedKeyWords
        listClassMemberKeywords.insert(contentsOf: listClassNames, at: 0)

        declarationKeywordList = listClassMemberKeywords

        // --------------------------------------------------
        // Collect declared variables
        // key: variable name
        // value: ASM bit prefix (e.g. s32, u64)
        // --------------------------------------------------
        var declaredVarList: [String: String] = [:]

        for code in codes {

            let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count <= 1 {
                continue
            }

            let keyword = splitCode[0] + " "

            // Must be a declaration keyword
            if !declarationKeywordList.contains(keyword) {
                continue
            }

            var varTypeBit =
                ConvertVarTypeToASMPrefix(keyword)

            // If this is a class instance variable, default to max bit size
            if varTypeBit.isEmpty,
               listClassNames.contains(keyword) {
                varTypeBit = "u" + String(TARGET_ARCH_BIT_SIZE)
            }

            if varTypeBit.isEmpty {
                continue
            }

            let varName = splitCode[1]
            declaredVarList[varName] = varTypeBit
        }

        return declaredVarList
    }
    
    /// <summary>
    /// Get the ASM prefix from the variable type that represents signed/unsigned
    /// e.g.
    /// int ---> REGs32
    /// uint ---> REGu32
    /// </summary>
    /// <param name="varType"></param>
    /// <returns></returns>
    private static func ConvertVarTypeToASMPrefix(
        _ varType: String,
        _ opCode: String = ""
    ) -> String {

        switch varType {

        case PH_RESV_KW_PTR:
            return opCode + "u" + String(TARGET_ARCH_BIT_SIZE)

        case PH_RESV_KW_VAR:
            return opCode + "s" + String(TARGET_ARCH_BIT_SIZE)

        case PH_RESV_KW_OBJECT:
            return opCode + "u" + String(TARGET_ARCH_BIT_SIZE)

        case PH_RESV_KW_CHAR:
            return opCode + "u" + String(CHAR_BIT_SIZE)

        case PH_RESV_KW_STRING:
            return opCode + "u" + String(TARGET_ARCH_BIT_SIZE)

        case PH_RESV_KW_BOOL:
            return opCode + "u" + String(BYTE_BIT_SIZE)

        case PH_RESV_KW_SBYTE:
            return opCode + "s" + String(BYTE_BIT_SIZE)

        case PH_RESV_KW_SHORT:
            return opCode + "s" + String(SHORT_BIT_SIZE)

        case PH_RESV_KW_INT:
            return opCode + "s" + String(INT_BIT_SIZE)

        case PH_RESV_KW_LONG:
            return opCode + "s" + String(LONG_BIT_SIZE)

        case PH_RESV_KW_BYTE:
            return opCode + "u" + String(BYTE_BIT_SIZE)

        case PH_RESV_KW_USHORT:
            return opCode + "u" + String(SHORT_BIT_SIZE)

        case PH_RESV_KW_UINT:
            return opCode + "u" + String(INT_BIT_SIZE)

        case PH_RESV_KW_ULONG:
            return opCode + "u" + String(LONG_BIT_SIZE)

        case PH_RESV_KW_FLOAT:
            return opCode + "s" + String(FLOAT_BIT_SIZE)

        case PH_RESV_KW_DOUBLE:
            return opCode + "s" + String(DOUBLE_BIT_SIZE)

        case PH_RESV_KW_DECIMAL:
            return opCode + "s" + String(DECIMAL_BIT_SIZE)

        default:
            return ""
        }
    }
    
    /// <summary>
    /// Gets the entire function list that are declared in the code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func GetDeclaredFunctionList(
        _ codes: [String]
    ) -> [String: String] {

        // key: function name
        // value: function return type keyword (e.g. "int", "void")
        var declaredFuncList: [String: String] = [:]

        for code in codes {

            let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            if splitCode.count <= 1 {
                continue
            }

            // Check if this is a function declaration keyword
            if !AllFunctionKeywords.contains(splitCode[0] + " ") {
                continue
            }

            let funcName = splitCode[1]
            let funcType = splitCode[0]

            declaredFuncList[funcName] = funcType
        }

        return declaredFuncList
    }

    /// <summary>
    /// Remove namespaces
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func RemoveNamespaces(
        _ codes: [String]
    ) -> [String] {

        var codes = codes

        var i = 0
        while i < codes.count {

            // Remove namespace and scope brackets
            if codes[i].contains(PH_RESV_KW_NAMESPACE) {

                let endScopeBracket =
                    FindEndScopeBlock(codes, i, "{", "}")

                // Safety guard (C# implicitly trusts structure)
                if endScopeBracket >= 0 {

                    // Remove:
                    // namespace xxx
                    // {
                    // }
                    codes[i] = ""

                    if i + 1 < codes.count {
                        codes[i + 1] = ""
                    }

                    if endScopeBracket < codes.count {
                        codes[endScopeBracket] = ""
                    }
                }
            }

            i += 1
        }

        // Let's clean the codes again by separating into tokens
        codes = RegenerateCleanCode(codes)
        return codes
    }
    
    /// <summary>
    /// Convert class member variables to pointer declarations/pointer vars
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func PrepareClassStructure(
        _ codes: [String]
    ) -> [String] {

        // --------------------------------------------------
        // Add empty code at the beginning (same as C#)
        // --------------------------------------------------
        var codes = codes
        codes.insert("", at: 0)

        // --------------------------------------------------
        // Scan and replace class / static class
        // --------------------------------------------------
        var i = 0
        while i < codes.count {

            // Replace static class to labels
            if codes[i].contains(PH_RESV_KW_STATIC_CLASS) {

                codes =
                    ReplaceClassToLabelAndConvertMemberVarToASM(
                        codes,
                        i,
                        true
                    )
                i += 1
                continue
            }

            // Replace normal class to labels
            if codes[i].contains(PH_RESV_KW_CLASS) {

                codes =
                    ReplaceClassToLabelAndConvertMemberVarToASM(
                        codes,
                        i,
                        false
                    )
                i += 1
                continue
            }

            i += 1
        }

        // --------------------------------------------------
        // Rebuild code string & re-tokenize
        // --------------------------------------------------
        var newCodes = ""
        for code in codes {
            newCodes += code
        }

        let newCodesArray = splitCodesToArray(newCodes)
        return newCodesArray
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
    private static func ReplaceClassToLabelAndConvertMemberVarToASM(
        _ codes: [String],
        _ startCodeIndex: Int,
        _ isStaticClass: Bool
    ) -> [String] {

        var codes = codes
        var startCodeIndex = startCodeIndex

        let classDefinitionString = isStaticClass ? PH_RESV_KW_STATIC_CLASS : PH_RESV_KW_CLASS

        // Remove class and scope brackets
        let endScopeBracket = FindEndScopeBlock(codes, startCodeIndex, "{", "}")
        if endScopeBracket == -1 {
            return codes
        }

        // Mirror C# behavior: replace PH_RESV_KW_CLASS even when static class is used
        var className = codes[startCodeIndex].replacingOccurrences(of: PH_RESV_KW_CLASS, with: "")
        className = className.trimmingCharacters(in: .whitespacesAndNewlines)

        // Find "{" and move startCodeIndex to the first token after it
        var i = startCodeIndex
        while i < endScopeBracket {
            if codes[i] == "{" {
                startCodeIndex = i + 1
                break
            }
            i += 1
        }

        // If not static class, insert pointer var declaration + POP at the top of the class block
        if !isStaticClass {
            let insertCode =
                PH_REG
                + "s"
                + String(TARGET_ARCH_BIT_SIZE)
                + " "
                + PH_ID
                + PH_POINTER
                + PH_ID
                + className
                + ";"
                + PH_POP
                + " "
                + className
                + ";"

            codes.insert(insertCode, at: startCodeIndex)

            // Store base size entry (byte size of target arch pointer)
            ActualClassMemberVarSizeList[className] = [TARGET_ARCH_BIT_SIZE / 8]
        }

        // Stores the total number of byte size this class has
        var totalAddressByteSize = 0

        // Convert class member variables to pointer variables
        i = startCodeIndex
        while i < endScopeBracket {

            // Stop/skip when reaching functions
            let splitFuncName = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if !splitFuncName.isEmpty,
               AllFunctionKeywords.contains(splitFuncName[0] + " ") {

                if !isStaticClass {
                    // For normal class, return instance address to caller
                    if i - 1 >= 0 {
                        codes[i - 1] += PH_PSH + " " + className + ";" + PH_RET + ";"
                    }
                    break
                }

                // For static class, skip function scope
                let endOfFunctionBlock = FindEndScopeBlock(codes, i, "{", "}")
                if endOfFunctionBlock == -1 {
                    i += 1
                } else {
                    i = endOfFunctionBlock + 1
                }
                continue
            }

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.isEmpty {
                i += 1
                continue
            }

            // If variable declaration found
            if ClassMemberReservedKeyWords.contains(splitCode[0] + " ") {

                // -------------------------------
                // Static class member vars: move decl/init to top of program
                // -------------------------------
                if isStaticClass {

                    var varDeclareCode =
                        ConvertVarDeclarationsToASM(
                            codes[i],
                            0,
                            !isStaticClass,
                            className
                        ) + ";"

                    // Remove declaration token and the next token (typically ';')
                    codes[i] = ""
                    if i + 1 < codes.count {
                        codes[i + 1] = ""
                    }
                    i += 1 // Skip semicolon code index

                    // Check if we have initiation code right after
                    var j = i
                    while j < endScopeBracket {

                        let splitFuncName2 = codes[j].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                        if !splitFuncName2.isEmpty,
                           AllFunctionKeywords.contains(splitFuncName2[0] + " ") {
                            break
                        } else if splitCode.count > 1, codes[j] == splitCode[1] {

                            // Search until end of statement found
                            var k = j
                            while k < endScopeBracket {
                                if codes[k] == ";" {
                                    var initCode = ""
                                    if i <= k {
                                        for m in i...k {
                                            initCode += codes[m]
                                            codes[m] = ""
                                        }
                                    }
                                    varDeclareCode += initCode
                                    i = k // Skip semicolon code index
                                    break
                                }
                                k += 1
                            }
                            break
                        }

                        j += 1
                    }

                    // Move to top of program
                    codes[0] += varDeclareCode

                } else {

                    // -------------------------------
                    // Normal class member vars: convert to pointer vars
                    // -------------------------------

                    // Get previous var's address size
                    var j = i
                    while j >= startCodeIndex {

                        // C# mirror: Regex.Match(codes[j], PH_REG + "(.*?) ").Value.Trim();
                        let regPattern =
                            NSRegularExpression.escapedPattern(for: PH_REG) + "(.*?) "

                        let varDeclareCode =
                            codes[j]
                                .firstMatchString(regPattern)
                                .trimmingCharacters(in: .whitespacesAndNewlines)

                        if !varDeclareCode.isEmpty, varDeclareCode.contains(PH_REG) {

                            if let list = ActualClassMemberVarSizeList[className],
                               let last = list.last {
                                totalAddressByteSize += last
                            }

                            break
                        }

                        j -= 1
                    }

                    // Convert declaration to ASM and replace at the same position
                    codes[i] =
                        ConvertVarDeclarationsToASM(
                            codes[i],
                            totalAddressByteSize,
                            !isStaticClass,
                            className
                        )

                    // Store the actual member sizes
                    if var list = ActualClassMemberVarSizeList[className] {
                        let splitMemberVarCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                        if !splitMemberVarCode.isEmpty {
                            var bitStr = splitMemberVarCode[0]
                            bitStr = bitStr.replacingOccurrences(of: PH_REG, with: "")
                            bitStr = bitStr.replacingOccurrences(of: "u", with: "")
                            bitStr = bitStr.replacingOccurrences(of: "s", with: "")
                            if let bitSize = Int(bitStr) {
                                list.append(bitSize / 8)
                                ActualClassMemberVarSizeList[className] = list
                            }
                        }
                    }

                    // Treat all non-static class member vars as address pointers
                    codes[i] =
                        codes[i].replacingOccurrences(
                            of: NSRegularExpression.escapedPattern(for: PH_REG) + #"(.*?)\d "#,
                            with: PH_REG + "s" + String(TARGET_ARCH_BIT_SIZE) + " ",
                            options: .regularExpression
                        )

                    codes[i] =
                        codes[i].replacingOccurrences(
                            of: NSRegularExpression.escapedPattern(for: PH_VAR) + #"(.*?)\d "#,
                            with: PH_VAR + "s" + String(TARGET_ARCH_BIT_SIZE) + " ",
                            options: .regularExpression
                        )

                    i += 1 // Skip semicolon code index

                    // Add pointer placeholder to any member var references within the class block
                    if splitCode.count > 1 {
                        let memberVarName = splitCode[1]
                        var jj = i
                        while jj < endScopeBracket {
                            if codes[jj] == memberVarName {
                                codes[jj] = PH_ID + PH_POINTER + PH_ID + codes[jj]
                            }
                            jj += 1
                        }
                    }
                }
            }

            i += 1
        }

        return codes
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
    private static func ConvertVarDeclarationsToASM(
        _ codes: [String],
        _ declaredVarList: [String: String]
    ) -> [String] {

        // Get all function parameter list within the code
        let declaredFuncParamList =
            GetAllFunctionParameterList(codes)

        var codes = codes

        for i in 0..<codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count <= 1 {
                continue
            }

            // Must be a variable declaration keyword
            if !ClassMemberReservedKeyWords.contains(splitCode[0] + " ") {
                continue
            }

            // Get variable bit type
            guard let varTypeBit = declaredVarList[splitCode[1]] else {
                continue
            }

            // Check to see if we have an array pointer
            var pointerIdentifier = ""

            if splitCode[0] + " " == PH_RESV_KW_PTR {

                // If this variable is inside a function parameter,
                // we don't want to convert it to a pointer
                if !declaredFuncParamList.contains(splitCode[1]) {
                    pointerIdentifier = PH_ID + PH_POINTER + PH_ID
                }
            }

            // Replace declaration with ASM code
            codes[i] = ReplaceFirstOccurance(
                codes[i],
                splitCode[0] + " ",
                ConvertVarTypeToASMPrefix(
                    splitCode[0] + " ",
                    PH_REG
                ) + " " + pointerIdentifier
            )
        }

        return codes
    }
    
    /// Converts variable declarations inside a single statement to ASM code
    /// C# equivalent:
    /// ConvertVarDeclarationsToASM(string code, int previousAddressByteSize, bool isConvertToPointerVar, string currentClassName)
    private static func ConvertVarDeclarationsToASM(
        _ code: String,
        _ previousAddressByteSize: Int = 0,
        _ isConvertToPointerVar: Bool = false,
        _ currentClassName: String = ""
    ) -> String {

        let splitCode = code.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
        if splitCode.count == 2 {

            let varType = splitCode[0] + " "
            var varCmd = ""

            let ptrSymbol =
                isConvertToPointerVar
                ? " " + PH_ID + PH_POINTER + PH_ID
                : " "

            // --------------------------------------------
            // Resolve class name from full variable path
            // --------------------------------------------
            let splitMemberVarName =
                splitCode[1].split(separator: ".").map(String.init)

            var className = ""

            if splitMemberVarName.count > 1 {
                for i in 0..<(splitMemberVarName.count - 1) {
                    className += splitMemberVarName[i] + "."
                }
                // remove trailing "."
                className.removeLast()
            } else {
                className = currentClassName
            }

            // If currentClassName is explicitly given, override
            if !currentClassName.isEmpty {
                className = currentClassName
            }

            // --------------------------------------------
            // Convert variable type to ASM prefix
            // --------------------------------------------
            let varTypeBit = ConvertVarTypeToASMPrefix(varType)

            switch varType {

            case PH_RESV_KW_PTR,
                 PH_RESV_KW_VAR,
                 PH_RESV_KW_OBJECT,
                 PH_RESV_KW_STRING,
                 PH_RESV_KW_CHAR,
                 PH_RESV_KW_BOOL,
                 PH_RESV_KW_SBYTE,
                 PH_RESV_KW_SHORT,
                 PH_RESV_KW_INT,
                 PH_RESV_KW_LONG,
                 PH_RESV_KW_BYTE,
                 PH_RESV_KW_USHORT,
                 PH_RESV_KW_UINT,
                 PH_RESV_KW_ULONG,
                 PH_RESV_KW_FLOAT,
                 PH_RESV_KW_DOUBLE,
                 PH_RESV_KW_DECIMAL:

                varCmd = PH_REG + varTypeBit + ptrSymbol

            default:
                break
            }

            // --------------------------------------------
            // Address assignment code for pointer vars
            // --------------------------------------------
            var asmAddressAssignmentCode = ""

            if isConvertToPointerVar {

                asmAddressAssignmentCode =
                    ";"
                    + PH_VAR
                    + "s"
                    + String(TARGET_ARCH_BIT_SIZE)
                    + " "
                    + splitCode[1]
                    + ";"
                    + PH_MOV
                    + " "
                    + className
                    + ";"
                    + PH_ADD
                    + " "
                    + (previousAddressByteSize == 0
                       ? ""
                       : String(previousAddressByteSize))
            }

            return varCmd + splitCode[1] + asmAddressAssignmentCode
        }

        return code
    }

    /// <summary>
    /// Get all function parameter list within the code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func GetAllFunctionParameterList(
        _ codes: [String]
    ) -> [String] {

        var declaredFuncParamList: [String] = []

        var i = 0
        while i < codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            // Must be a function declaration keyword
            if splitCode.isEmpty
                || !AllFunctionKeywords.contains(splitCode[0] + " ") {
                i += 1
                continue
            }

            // "(" should start right after function declaration
            let roundBracketStartIndex = i + 1
            let roundBracketEndIndex =
                FindEndScopeBlock(
                    codes,
                    roundBracketStartIndex,
                    "(",
                    ")"
                )

            if roundBracketEndIndex == -1 {
                i += 1
                continue
            }

            // Collect parameters between ( and )
            var parameters = ""
            if roundBracketStartIndex + 1 <= roundBracketEndIndex - 1 {
                for j in (roundBracketStartIndex + 1)...(roundBracketEndIndex - 1) {
                    parameters += codes[j]
                }
            }

            if parameters.isEmpty {
                i += 1
                continue
            }

            // Split parameters by comma
            let splitParams =
                parameters.split(separator: ",").map(String.init)

            for param in splitParams {
                // Each param is "type varName"
                let splitParamItem =
                    param.split(separator: " ", omittingEmptySubsequences: false).map(String.init)

                if splitParamItem.count > 1 {
                    declaredFuncParamList.append(splitParamItem[1])
                }
            }

            i += 1
        }

        return declaredFuncParamList
    }
    
    /// Convert increment/decrement statements on its own to one line
    /// (e.g.)
    /// [0] i  [1] +  [2] +  ----> [0] i++
    /// [0] -  [1] -  [2] i  ----> [0] --i
    private static func ConvertIncDecToOneLine(
        _ codes: [String],
        _ operatorSingleSymbol: String
    ) -> [String] {

        var codes = codes

        var i = 0
        while i < codes.count {

            if codes[i] != operatorSingleSymbol {
                i += 1
                continue
            }

            if i + 1 >= codes.count || codes[i + 1] != operatorSingleSymbol {
                i += 1
                continue
            }

            // Remove the two operator tokens
            codes[i] = ""
            codes[i + 1] = ""

            // Determine pre / post increment
            if i + 2 < codes.count, codes[i + 2] == ";" {
                // Post-increment: i++
                if i - 1 >= 0 {
                    codes[i - 1] += operatorSingleSymbol + operatorSingleSymbol
                }
            } else if i + 2 < codes.count {
                // Pre-increment: ++i
                codes[i + 2] =
                    operatorSingleSymbol
                    + operatorSingleSymbol
                    + codes[i + 2]
            }

            i += 1
        }

        return codes
    }
    
    /// Resolve stand-alone increment/decrement codes
    private static func ResolveStandAloneIncrementDecrement(
        _ codes: [String]
    ) -> [String] {

        var codeList = codes
        var i = 0

        while i < codeList.count {

            // 通常は次へ
            defer { i += 1 }

            // ------------------------------------------
            // Ignore expressions: skip until ';'
            // ------------------------------------------
            if codeList[i] == "=" {

                var j = i
                while j < codeList.count {
                    if codeList[j] == ";" {
                        i = j
                        break
                    }
                    j += 1
                }
                continue
            }

            // ------------------------------------------
            // Stand-alone increment (＋)
            // ------------------------------------------
            if codeList[i].contains("＋") {

                var variable = ""

                // Post-increment: i ＋ ＋ ;
                if i + 1 < codeList.count, codeList[i + 1] == ";" {
                    variable = codeList[i - 1]
                    codeList[i] = "="
                    codeList.insert(variable, at: i + 1)
                    codeList.insert("+", at: i + 2)
                    codeList.insert("1", at: i + 3)

                } else if i + 1 < codeList.count {
                    // Pre-increment: ＋ ＋ i
                    variable = codeList[i + 1]
                    codeList[i] = variable
                    codeList.insert("=", at: i + 1)
                    codeList.insert("+", at: i + 3)
                    codeList.insert("1", at: i + 4)
                }

                // C# の i -= 2 と完全一致
                i -= 2
                continue
            }

            // ------------------------------------------
            // Stand-alone decrement (—)
            // ------------------------------------------
            if codeList[i].contains("—") {

                var variable = ""

                // Post-decrement
                if i + 1 < codeList.count, codeList[i + 1] == ";" {
                    variable = codeList[i - 1]
                    codeList[i] = "="
                    codeList.insert(variable, at: i + 1)
                    codeList.insert("-", at: i + 2)
                    codeList.insert("1", at: i + 3)

                } else if i + 1 < codeList.count {
                    // Pre-decrement
                    variable = codeList[i + 1]
                    codeList[i] = variable
                    codeList.insert("=", at: i + 1)
                    codeList.insert("-", at: i + 3)
                    codeList.insert("1", at: i + 4)
                }

                // 再評価
                i -= 2
                continue
            }
        }

        return codeList
    }
    
    /// Convert mathematical expressions to ASM code
    private static func ConvertExpressionsToASM(
        _ codes: [String],
        _ declaredVarList: [String: String],
        _ instanceObjList: [String: String],
        _ declaredFuncList: [String: String]
    ) -> [String] {

        var codes = codes

        // Get all class declarations
        let classDeclarationList =
            GetAllKeywordDeclarations(
                codes,
                PH_RESV_KW_CLASS,
                false
            )

        var i = 0
        while i < codes.count {

            if codes[i] != "=" {
                i += 1
                continue
            }

            // Extract full expression and remove original tokens
            let expression =
                GetFullExpressionStatementFromAssignmentOperator(
                    &codes,
                    i,
                    true
                )

            // Replace "=" token with converted ASM expression
            codes[i] =
                ConvertExpressionStatementToASM(
                    expression,
                    codes,
                    declaredVarList,
                    instanceObjList,
                    declaredFuncList,
                    classDeclarationList
                )

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Converts a single expression statement to ASM code
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private static func ConvertExpressionStatementToASM(
        _ expression: String,
        _ codes: [String],
        _ declaredVarList: [String: String],
        _ instanceObjList: [String: String],
        _ declaredFuncList: [String: String],
        _ classDeclarationList: [String]
    ) -> String {
        // --------------------------------------------------
        // Temporary store the original expression
        // --------------------------------------------------
        var expression = expression
        let originalExpression = expression

        // --------------------------------------------------
        // Ignore expressions inside double quotations (RPN)
        // --------------------------------------------------
        var expRPN = ""
        if let match = expression.range(
            of: #""(.*?)""#,
            options: .regularExpression
        ) {
            expRPN = String(expression[match])
        }

        if !expRPN.isEmpty {
            expression = expression.replacingOccurrences(
                of: expRPN,
                with: PH_ID + PH_EXP_STR + PH_ID
            )
        }

        // --------------------------------------------------
        // Operator replacement
        // --------------------------------------------------
        let operatorList = [
            "==", "<=", ">=", "!=", "++", "--",
            "<", ">", "+", "-", "*", "/", "%",
            "&&", "||", "&", "|", "^", "="
        ]

        // Protect full-width increment/decrement
        expression = expression
            .replacingOccurrences(of: "＋", with: PH_ID + "INCREMENT")
            .replacingOccurrences(of: "—", with: PH_ID + "DECREMENT")

        for op in operatorList {
            expression = expression.replacingOccurrences(
                of: op,
                with: " " + ConvertOperatorToASM(op) + " "
            )
        }

        // Restore full-width increment/decrement
        expression = expression
            .replacingOccurrences(of: PH_ID + "INCREMENT", with: "＋")
            .replacingOccurrences(of: PH_ID + "DECREMENT", with: "—")

        // --------------------------------------------------
        // Split code up, and convert variable assignment to "VAR{u|s}xxx varname" format
        // --------------------------------------------------
        var splitCode =
            expression
                .split(separator: " ", omittingEmptySubsequences: false)
                .map { String($0) }

        // --------------------------------------------------
        // Find and resolve any post/pre increment/decrement
        // --------------------------------------------------
        splitCode = ResolveIncDecInExpression(splitCode, declaredVarList, 1)

        // --------------------------------------------------
        // Find first variable token and prefix it with VARxx
        // --------------------------------------------------
        for i in 0..<splitCode.count {

            // Remove pointer indicator for now
            var varToCheck =
                splitCode[i].replacingOccurrences(
                    of: PH_ID + PH_POINTER + PH_ID,
                    with: ""
                )

            // Remove square brackets for now
            varToCheck = varToCheck.replacingOccurrences(
                of: #"\[(.*?)\]$"#,
                with: "",
                options: .regularExpression
            )

            if declaredVarList[varToCheck] == nil {

                // No variable found in our declared list, so now we check if this variable is an instance object
                var checkInstanceObj = ""
                let splitVar = varToCheck.split(separator: ".").map(String.init)
                if splitVar.count == 1 {
                    continue
                }

                for j in 0..<(splitVar.count - 1) {
                    checkInstanceObj += splitVar[j] + "."
                }

                // Remove the trailing dot
                if !checkInstanceObj.isEmpty {
                    checkInstanceObj.removeLast()
                }

                var resolvedClassScopeObj = checkInstanceObj

                // Now check again with the resolved scope
                if instanceObjList[checkInstanceObj] == nil {

                    // If the variable does not exist in the instance object list, it might be a reference to the member of the class
                    // So, resolve the entire function call scope path to absolute class scope path
                    resolvedClassScopeObj = GetResolvedNestedInstanceVar(checkInstanceObj, instanceObjList)

                    if instanceObjList[resolvedClassScopeObj] == nil {
                        continue
                    }
                }

                // It was an instance object, so now we check if the original variable exists as a class member variable
                if let resolvedType = instanceObjList[resolvedClassScopeObj] {
                    varToCheck = ReplaceFirstOccurance(varToCheck, checkInstanceObj, resolvedType)
                }

                if declaredVarList[varToCheck] == nil {
                    continue
                }

                // Variable exists! We need to set this a pointer
                splitCode[i] =
                    PH_VAR
                    + declaredVarList[varToCheck]!
                    + " "
                    + PH_ID
                    + PH_POINTER
                    + PH_ID
                    + splitCode[i]

                break
            }

            // If we have a variable that matches with our variable list item, get the bit size from the list and convert to ASM code
            splitCode[i] = PH_VAR + declaredVarList[varToCheck]! + " " + splitCode[i]
            break
        }

        // --------------------------------------------------
        // Rebuild expression string from splitCode
        // --------------------------------------------------
        expression = ""
        for i in 0..<splitCode.count {
            expression += splitCode[i] + " "
        }
        expression = expression.trimmingCharacters(in: .whitespaces)

        
        // --------------------------------------------------
        // RPN expressions handling
        // --------------------------------------------------
        // Example:
        // x = (a == b || c && d) + result_pne;
        // ---->
        // x = "a#b#==#c#d#&&#||#result_pne#+";
        if !expRPN.isEmpty {

            // Remove quotes
            expRPN = expRPN.replacingOccurrences(of: "\"", with: "")

            var splitExpRPN =
                expRPN
                    .split(separator: "#")
                    .map { String($0) }

            // ----------------------------------------------
            // Check for instance variables and add pointer
            // ----------------------------------------------
            for i in 0..<splitExpRPN.count {

                if !splitExpRPN[i].contains(".") {
                    continue
                }

                let checkVarSplit =
                    splitExpRPN[i]
                        .split(separator: ".")
                        .map(String.init)

                var varToCheck = ""

                if checkVarSplit.count > 1 {
                    for j in 0..<(checkVarSplit.count - 1) {
                        varToCheck += checkVarSplit[j] + "."
                    }
                    // Remove trailing "."
                    varToCheck.removeLast()
                }

                if classDeclarationList.contains(varToCheck) {
                    splitExpRPN[i] =
                        PH_ID + PH_POINTER + PH_ID + splitExpRPN[i]
                }
            }

            // ----------------------------------------------
            // Generate ASM code from RPN
            // ----------------------------------------------
            var asmCode = ""

            for token in splitExpRPN {

                if Operators.contains(token) {

                    // Unary operators
                    if token == "!"
                        || token == "~"
                        || token == "UnaryMinus"
                        || token == "UnaryPlus" {

                        asmCode +=
                            " "
                            + PH_POP + " " + REG_SVAR0 + " "
                            + PH_VAR + "s" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_SVAR0 + " "
                            + ConvertOperatorToASM(token) + " "
                            + PH_PSH + " " + REG_SVAR0 + " "

                    } else {

                        // Binary operators (FILO: reverse POP order)
                        asmCode +=
                            " "
                            + PH_POP + " " + REG_SVAR1 + " "
                            + PH_POP + " " + REG_SVAR0 + " "
                            + PH_VAR + "s" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_SVAR0 + " "
                            + ConvertOperatorToASM(token) + " " + REG_SVAR1 + " "
                            + PH_PSH + " " + REG_SVAR0 + " "
                    }

                } else {
                    // Operand
                    asmCode += PH_PSH + " " + token + " "
                }
            }

            // ----------------------------------------------
            // Final POP: store result into REG_SVAR0
            // ----------------------------------------------
            asmCode += PH_POP + " " + REG_SVAR0 + " "

            // Replace placeholder with REG_SVAR0
            expression =
                expression.replacingOccurrences(
                    of: PH_ID + PH_EXP_STR + PH_ID,
                    with: REG_SVAR0
                )

            // Prepend generated ASM code
            expression = asmCode + expression

            // ----------------------------------------------
            // Resolve any post/pre increment/decrement again
            // ----------------------------------------------
            var splitCodeExpression =
                expression
                    // Mirror C# `Split(' ')` behavior: KEEP empty entries created by double spaces.
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map { String($0) }

            splitCodeExpression =
                ResolveIncDecInExpression(
                    splitCodeExpression,
                    declaredVarList,
                    0
                )

            expression = ""
            for token in splitCodeExpression {
                expression += token + " "
            }
        }
        
        // --------------------------------------------------
        // Sequential / stand-alone function calls
        // --------------------------------------------------
        if expression.contains("#") {
            // TODO: Sequential function calls
        } else if expression.contains(")") {

            // ----------------------------------------------
            // Stand-alone function call
            // ----------------------------------------------
            // Note: There will only be one function call per statement
            let funcEndIndex = expression.firstIndex(of: ")")!
            let funcEndPos = expression.distance(from: expression.startIndex, to: funcEndIndex)

            let funcStartPos =
                ObtainFuncVarStartIndex(
                    expression,
                    funcEndPos,
                    "(",
                    false
                )

            let startIdx = expression.index(expression.startIndex, offsetBy: funcStartPos)
            let endIdx   = expression.index(expression.startIndex, offsetBy: funcEndPos)

            let function =
                String(expression[startIdx...endIdx])

            // ----------------------------------------------
            // Construct parameter PUSH ASM code
            // ----------------------------------------------
            var asmParamPushCode = ""

            if function.contains(",") {

                // Multiple parameters
                let paramMatch =
                    function.firstMatchString(#"(?<=\().+?(?=\))"#)

                let funcParams =
                    paramMatch
                        .split(separator: ",")
                        .map { String($0) }

                for param in funcParams {
                    asmParamPushCode += PH_PSH + " " + param + " "
                }

            } else if !function.contains("()") {

                // Single parameter
                let funcParam =
                    function.firstMatchString(#"(?<=\().+?(?=\))"#)

                asmParamPushCode =
                    PH_PSH + " " + funcParam + " "

            } else {
                // No parameters → do nothing
            }

            // ----------------------------------------------
            // Extract function name (remove parentheses)
            // ----------------------------------------------
            let functionName =
                function.replacingOccurrences(
                    of: #"\((.*?)\)"#,
                    with: "",
                    options: .regularExpression
                )

            // ----------------------------------------------
            // Determine return register type
            // ----------------------------------------------
            var retVarType = ""

            // NEW keyword → constructor → unsigned
            if expression.contains(PH_ID + PH_NEW + PH_ID) {
                retVarType = REG_UVAR0
            } else {
                retVarType = REG_SVAR0
            }

            // ----------------------------------------------
            // Resolve function scope for instance calls
            // ----------------------------------------------
            let splitFuncName =
                functionName
                    .split(separator: ".")
                    .map(String.init)

            var chkFunctionName = ""

            if splitFuncName.count > 1 {
                for i in 0..<(splitFuncName.count - 1) {
                    chkFunctionName += splitFuncName[i] + "."
                }
                chkFunctionName.removeLast()
            }

            var funcWithRealClassName = ""

            if let realClassName = instanceObjList[chkFunctionName] {

                funcWithRealClassName =
                    realClassName
                    + "."
                    + splitFuncName.last!

                if let funcType = declaredFuncList[funcWithRealClassName] {

                    if funcType + " " == PH_RESV_KW_FUNCTION_PTR
                        || funcType + " " == PH_RESV_KW_FUNCTION_ULONG {

                        retVarType = REG_UVAR0
                    }
                }

            } else if let funcType = declaredFuncList[functionName] {

                if funcType + " " == PH_RESV_KW_FUNCTION_PTR
                    || funcType + " " == PH_RESV_KW_FUNCTION_ULONG {

                    retVarType = REG_UVAR0
                }
            }

            // ----------------------------------------------
            // Construct final ASM expression
            // ----------------------------------------------
            expression =
                asmParamPushCode
                + PH_JMS + " " + functionName + " "
                + PH_POP + " " + retVarType + " "
                + expression.replacingOccurrences(of: function, with: "")
                + " "
                + retVarType
        } else if expression.contains("]") {
            
            // --------------------------------------------------
            // Array instantiation or array index reference
            // --------------------------------------------------

            // Instantiating an array of objects, which means we do not have any parameters to
            // (e.g.) VARu64 N.Program.Main.b MOV 𒀭PH_NEW𒀭N.INT32[xxxxx]
            // (where xxxxx is the number of arrays to create)
            let funcEndIndex = expression.firstIndex(of: "]")!
            let funcEndPos = expression.distance(from: expression.startIndex, to: funcEndIndex)

            let funcStartPos = ObtainFuncVarStartIndex(expression, funcEndPos, "[", false)

            let startIdx = expression.index(expression.startIndex, offsetBy: funcStartPos)
            let endIdx = expression.index(expression.startIndex, offsetBy: funcEndPos)
            
            let instanceObjWithParam = String(expression[startIdx...endIdx])
            var instanceObjWithoutParam = instanceObjWithParam.replacingOccurrences(
                of: #"\[(.*?)\]"#,
                with: "",
                options: .regularExpression
            )

            var instanceObjName = instanceObjWithoutParam.replacingOccurrences(
                of: PH_ID + PH_NEW + PH_ID,
                with: ""
            )

            var arrayItem = instanceObjWithParam.firstMatchString(#"(?<=\[).+?(?=\])"#)

            // If instanceObjWithoutParam does not contain a NEW keyword, this is not an instantiation, but just an array index reference
            if !instanceObjWithoutParam.contains(PH_ID + PH_NEW + PH_ID) {

                // Get the size of this array (it will be an instance variable)
                var instanceVar = instanceObjName

                // Replace any pointer keywords
                instanceVar = instanceVar.replacingOccurrences(
                    of: PH_ID + PH_POINTER + PH_ID,
                    with: ""
                )

                if let t = instanceObjList[instanceVar] {
                    instanceObjName = t
                } else {
                    // If the variable does not exist in the instance object list, it might be a reference to the member of the class
                    // So, resolve the entire function call scope path to absolute class scope path
                    let splitFuncName = instanceVar.split(separator: ".").map(String.init)
                    var chkFunctionName = ""
                    if splitFuncName.count > 1 {
                        for i in 0..<(splitFuncName.count - 1) {
                            chkFunctionName += splitFuncName[i] + "."
                        }
                        chkFunctionName.removeLast()
                    }

                    if instanceObjList[chkFunctionName] == nil {
                        chkFunctionName = GetResolvedNestedInstanceVar(chkFunctionName, instanceObjList)
                    }

                    if let resolved = instanceObjList[chkFunctionName] {
                        instanceObjName = resolved
                    }
                }

                // Always the byte size of the class pointer / primitive size
                var byteSize = 0

                if instanceObjName + " " == PH_RESV_KW_PTR {
                    // If instanceObjName has "ptr𒀭" as object type, we don't really know the bytesize for it
                    // We will find it out later
                    byteSize = -1
                } else {
                    byteSize = GetBitSizeOfVarDeclaration(instanceObjName + " ") / 8
                    // If byteSize is zero, it means we are dealing with a class, so we assign the class pointer size
                    if byteSize == 0 { byteSize = TARGET_ARCH_BIT_SIZE / 8 }
                }

                // Convert array code referencing/assignment to array
                var splitArrayCode = expression.split(separator: " ", omittingEmptySubsequences: false).map(String.init)

                // We might have something like:
                // [0] result_xxx
                // [1] MOV
                // [2] VARu32
                // [3] N.Program.Main.i[xxx]
                // In this case, we need to remove [2]
                var ii = 0
                while ii < splitArrayCode.count {
                    if splitArrayCode[ii] != PH_MOV {
                        ii += 1
                        continue
                    }

                    if ii + 1 < splitArrayCode.count {
                        let pattern = NSRegularExpression.escapedPattern(for: PH_VAR) + #"[u|s][0-9]"#
                        if splitArrayCode[ii + 1].range(of: pattern, options: .regularExpression) != nil {
                            splitArrayCode[ii + 1] = ""

                            var rebuilt = ""
                            for item in splitArrayCode {
                                rebuilt += item + " "
                            }

                            splitArrayCode = rebuilt.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                            // do not advance ii too aggressively after rebuild
                            ii = 0
                            continue
                        }
                    }

                    ii += 1
                }

                // Resolve array access
                for i in 0..<splitArrayCode.count {
                    if !splitArrayCode[i].contains("[") {
                        continue
                    }

                    // Note that instanceObjWithoutParam might be a nested pointer so we might need to reference
                    // the address it points to by adding a # sign in front of the instanceObjWithoutParam
                    // So, first of all we need to get all classes inside our instance object list
                    var listOfClass: [String] = []
                    for kvp in instanceObjList {
                        if listOfClass.contains(kvp.value) { continue }
                        if ClassMemberReservedKeyWords.contains(kvp.value + " ") { continue }
                        listOfClass.append(kvp.value)
                    }

                    var isNestedPointerObj = false
                    for className in listOfClass {
                        if instanceObjWithoutParam.count < className.count { continue }
                        if instanceObjWithoutParam == className {
                            isNestedPointerObj = true
                            break
                        } else if instanceObjWithoutParam.count >= className.count + 1 {
                            let prefix = className + "."
                            if instanceObjWithoutParam.hasPrefix(prefix) {
                                isNestedPointerObj = true
                                break
                            }
                        }
                    }

                    // We have an address passed on to a function scope variable, so we need to get the type of pointer here
                    if byteSize == -1 {
                        isNestedPointerObj = false
                        if let t = ActualClassMemberPointerVarTypeList[instanceObjWithoutParam] {
                            byteSize = GetBitSizeOfVarDeclaration(t + " ") / 8
                        } else {
                            byteSize = TARGET_ARCH_BIT_SIZE / 8
                        }
                    }

                    var referAddressString = ""
                    if isNestedPointerObj { referAddressString = "#" }

                    // If array variable has a variable operation operand "VARxxx" in front of it, then it's an assignment to an array variable
                    if i - 1 >= 0 {
                        let pattern = NSRegularExpression.escapedPattern(for: PH_VAR) + #"[u|s][0-9]"#
                        if splitArrayCode[i - 1].range(of: pattern, options: .regularExpression) != nil {

                            let asmArrayAssignCode =
                                REG_UVAR0 + " " + PH_MOV + " " + arrayItem + " " + PH_MLT + " " + String(byteSize)
                                + " " + PH_ADD + " " + referAddressString + instanceObjWithoutParam

                            splitArrayCode[i] = asmArrayAssignCode

                            if i + 2 < splitArrayCode.count {
                                splitArrayCode[i + 1] = PH_PSH + " " + splitArrayCode[i + 2]

                                // If instanceObjWithoutParam is declared as a pointer, we just use the value of REG_UVAR0
                                if (instanceObjList[instanceObjWithoutParam] ?? "") + " " == PH_RESV_KW_PTR {
                                    splitArrayCode[i + 2] = PH_POP + " " + REG_UVAR0
                                } else {
                                    splitArrayCode[i + 2] = PH_POP + " " + PH_ID + PH_POINTER + PH_ID + REG_UVAR0
                                }

                                expression = ""
                                break
                            }

                        } else {
                            // Else, the array is referenced by some other operation
                            let asmArrayAssignCode =
                                PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR0 + " "
                                + PH_MOV + " " + arrayItem + " " + PH_MLT + " " + String(byteSize)
                                + " " + PH_ADD + " " + referAddressString + instanceObjWithoutParam + " "

                            splitArrayCode[i] = PH_ID + PH_POINTER + PH_ID + REG_UVAR0
                            expression = asmArrayAssignCode
                            break
                        }
                    }
                }

                for item in splitArrayCode {
                    expression += item + " "
                }

            } else {

                // --------------------------------------------------
                // Array instantiation (NEW ... [size])
                // --------------------------------------------------

                // Create Memory allocation command for the number of arrays
                var byteSize = CalculateClassMemberVarSize(codes, instanceObjName)

                // Note that if a new array is created for a non-class (primitive types like int, byte, etc)
                // we will need to get the primitive size
                if byteSize == 0 {
                    byteSize = GetBitSizeOfVarDeclaration(instanceObjName + " ") / 8
                }

                // If the array instantiated is user-defined class.. (if not primitive types)
                if !ClassMemberReservedKeyWords.contains(instanceObjName + " ") {
                    // If classWithParam contains a NEW and square brackets, we can say that an instance is created as an array
                    // Therefore, multiple arrays are created which are just pointers
                    if instanceObjWithParam.contains("[") && instanceObjWithParam.contains(PH_ID + PH_NEW + PH_ID) {
                        byteSize = TARGET_ARCH_BIT_SIZE / 8
                    }
                }

                let numArrayToCreate = arrayItem

                // If array contains multi-dimentional array
                if arrayItem.contains(",") {
                    let items = arrayItem.split(separator: ",").map(String.init)
                    arrayItem = ""
                    for item in items {
                        arrayItem += PH_MLT + " " + item + " "
                    }
                } else {
                    arrayItem = PH_MLT + " " + arrayItem + " "
                }

                var instanceObjCode = ReplaceFirstOccurance(expression, instanceObjWithParam, "0")

                // Create a new variable to store the function call sequence results and insert into the expression list
                let tmpVar = RandomString(RND_ALPHABET_STRING_LEN_MAX, true, TmpVarIdentifier)

                var newArrayASMCode =
                    PH_REG + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + tmpVar + " "
                    + PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + tmpVar + " "
                    + PH_MOV + " " + numArrayToCreate + " "
                    + PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR1 + " "
                    + PH_MOV + " " + tmpVar + " " + PH_MLT + " " + String(byteSize) + " "
                    + instanceObjCode + " "
                    + PH_NOP + " " + "lbl_" + tmpVar + ";"

                // Reduce instanceObjCode to: "VAR.. <varName>" (first two tokens)
                let parts = instanceObjCode.split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                if parts.count >= 2 {
                    instanceObjCode = parts[0] + " " + parts[1]
                }

                newArrayASMCode +=
                    PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR0 + " "
                    + PH_MOV + " " + String(byteSize) + " "
                    + instanceObjCode + " "
                    + PH_PSH + " " + REG_UVAR0 + " "
                    + PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " "
                    + PH_POP + " " + REG_SVAR0 + " "
                    + PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + tmpVar + " "
                    + PH_SUB + " 1 " + PH_SKE + " 0 " + PH_JMP + " lbl_" + tmpVar + " "
                    + instanceObjCode + " " + PH_SUB + " " + REG_UVAR1 + " "
                    + PH_ADD + " " + String(byteSize) + " "

                // Add code to call the VM's register malloc group size functionality
                newArrayASMCode +=
                    PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR0 + " "
                    + PH_MOV + " " + numArrayToCreate + " " + PH_MLT + " " + String(byteSize) + " "
                    + PH_PSH + " " + (parts.count >= 2 ? parts[1] : "") + " "
                    + PH_PSH + " " + REG_UVAR0 + " "
                    + PH_JMS + " " + PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE + " "
                    + PH_POP + " " + REG_SVAR0 + " "

                expression = newArrayASMCode
            }
        }

        // --------------------------------------------------
        // Instantiation handling (mirror C# goto logic)
        // --------------------------------------------------
        while true {

            // If expression contains "JMS 𒀭PH_NEW𒀭XXXX"
            let instKey = PH_JMS + " " + PH_ID + PH_NEW + PH_ID
            let instantiateStartIndex = expression.range(of: instKey)

            if instantiateStartIndex == nil {
                break
            }

            // Get start position
            let startPos =
                expression.distance(
                    from: expression.startIndex,
                    to: instantiateStartIndex!.lowerBound
                )

            // Find end index (next space)
            let searchStart =
                expression.index(
                    expression.startIndex,
                    offsetBy: startPos + PH_JMS.count + 4
                )

            guard let spaceRange =
                expression.range(
                    of: " ",
                    range: searchStart..<expression.endIndex
                )
            else {
                break
            }

            let endPos =
                expression.distance(
                    from: expression.startIndex,
                    to: spaceRange.lowerBound
                )

            let instCurrentCode =
                String(
                    expression[
                        expression.index(expression.startIndex, offsetBy: startPos)
                        ..<
                        expression.index(expression.startIndex, offsetBy: endPos)
                    ]
                )

            // Get LHS variable name (before "=")
            let newObjName =
                String(
                    originalExpression.prefix(
                        upTo: originalExpression.firstIndex(of: "=")!
                    )
                ).trimmingCharacters(in: .whitespaces)

            // Resolve class name from NEW
            let className =
                originalExpression.firstMatchString(
                    "(?<=" + PH_ID + PH_NEW + PH_ID + ").+?(?=\\()"
                )

            let splitClassName =
                className.split(separator: ".").map(String.init)

            let constructorName =
                splitClassName.last ?? ""

            // --------------------------------------------------
            // Get member variable size list
            // --------------------------------------------------
            guard let memberVarSizeList =
                ActualClassMemberVarSizeList[className]
            else {
                break
            }

            var totalMemberVarSize = 0
            for size in memberVarSizeList {
                totalMemberVarSize += size
            }

            // --------------------------------------------------
            // Generate instantiation ASM code
            // --------------------------------------------------
            var instantiationASMCode =
                PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR1 + " " +
                PH_MOV + " " + String(totalMemberVarSize) + " " +
                PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + newObjName + " " +
                PH_MOV + " 0 "

            for varSize in memberVarSizeList {
                instantiationASMCode +=
                    PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + newObjName + " " +
                    PH_PSH + " " + String(varSize) + " " +
                    PH_JMS + " " + PH_RESV_SYSFUNC_MALLOC + " " +
                    PH_POP + " " + REG_SVAR0 + " "
            }

            // Adjust pointer to beginning of allocation
            instantiationASMCode +=
                PH_VAR + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + newObjName + " " +
                PH_SUB + " " + REG_UVAR1 + " " +
                PH_ADD + " " + String(memberVarSizeList.last!) + " " +
                PH_PSH + " " + newObjName + " " +
                PH_JMS + " " + className + " " +
                PH_JMS + " " + className + "." + constructorName + " " +
                PH_POP + " " + REG_SVAR0 + " "

            // Replace instantiation sequence
            expression =
                ReplaceFirstOccurance(
                    expression,
                    instCurrentCode,
                    instantiationASMCode
                )

            // Register malloc group size (for GC)
            expression +=
                " " + PH_PSH + " " + newObjName + " " +
                PH_PSH + " " + String(totalMemberVarSize) + " " +
                PH_JMS + " " + PH_RESV_SYSFUNC_REG_MALLOC_GROUP_SIZE + " " +
                PH_POP + " " + REG_SVAR0 + " "

            // Loop again (mirror goto instantiationCheck)
        }
        
        return expression
    }
    
    private static func ConvertOperatorToASM(
        _ operatorStr: String
    ) -> String {

        switch operatorStr {
        case "UnaryMinus": return PH_UMN
        case "UnaryPlus": return PH_UPL
        // case "UnaryNot":
        //     return PH_UNT
        // case "BitwiseUnaryComplement":
        //     return PH_BUC
        case "==": return PH_LEQ
        case "<=": return PH_LSE
        case ">=": return PH_LGE
        case "!=": return PH_LNE
        case "<": return PH_LGS
        case ">": return PH_LGG
        case "+": return PH_ADD
        case "-": return PH_SUB
        case "*": return PH_MLT
        case "/": return PH_DIV
        case "%": return PH_MOD
        case "<<": return PH_SHL
        case ">>": return PH_SHR
        case "&&": return PH_CAN
        case "||": return PH_COR
        case "&": return PH_AND
        case "|": return PH_LOR
        case "^": return PH_XOR
        case "!": return PH_UNT
        case "~": return PH_BUC
        case "=": return PH_MOV
        default: return ""
        }
    }
    /// <summary>
    /// Find and resolve any post/pre increment/decrement
    /// </summary>
    /// <param name="splitCode"></param>
    /// <returns></returns>
    private static func ResolveIncDecInExpression(
        _ splitCode: [String],
        _ declaredVarList: [String: String],
        _ offset: Int = 0
    ) -> [String] {

        var splitCodeList = splitCode
        var i = 0

        while i < splitCodeList.count {

            let token = splitCodeList[i]

            let indexInc = token.firstIndex(of: "＋")
            let indexDec = token.firstIndex(of: "—")

            if indexInc == nil && indexDec == nil {
                i += 1
                continue
            }

            // --------------------------------------------------
            // Increment
            // --------------------------------------------------
            if indexInc != nil {

                let variable = token.replacingOccurrences(of: "＋", with: "")

                guard let varType = declaredVarList[variable] else {
                    i += 1
                    continue
                }

                // Pre-increment: ＋＋x
                if token.first == "＋" {
                    splitCodeList[i] = variable
                    splitCodeList.insert(
                        PH_VAR + varType + " " + variable + " " + PH_ADD + " 1 ",
                        at: i - 1 - offset
                    )
                }
                // Post-increment: x＋＋
                else {
                    splitCodeList[i] = variable
                    splitCodeList.insert(
                        PH_VAR + varType + " " + variable + " " + PH_ADD + " 1 ",
                        at: i + 1
                    )
                }
            }

            // --------------------------------------------------
            // Decrement
            // --------------------------------------------------
            else {

                let variable = token.replacingOccurrences(of: "—", with: "")

                guard let varType = declaredVarList[variable] else {
                    i += 1
                    continue
                }

                // Pre-decrement: ——x
                if token.first == "—" {
                    splitCodeList[i] = variable
                    splitCodeList.insert(
                        PH_VAR + varType + " " + variable + " " + PH_SUB + " 1 ",
                        at: i - 1 - offset
                    )
                }
                // Post-decrement: x——
                else {
                    splitCodeList[i] = variable
                    splitCodeList.insert(
                        PH_VAR + varType + " " + variable + " " + PH_SUB + " 1 ",
                        at: i + 1
                    )
                }
            }

            i += 1
        }

        return splitCodeList
    }
    
    /// Calculates the total size (in bytes) of the member variables in the specified class
    /// - Parameters:
    ///   - codes: Intermediate codes
    ///   - className: Target class name
    ///   - memberVarSizeList: If specified, codes will be ignored and this list will be used instead
    /// - Returns: Total size in bytes
    private static func CalculateClassMemberVarSize(
        _ codes: [String],
        _ className: String,
        _ memberVarSizeList: [Int]? = nil
    ) -> Int {

        var total = 0
        var memberVarList: [Int]

        if let list = memberVarSizeList {
            memberVarList = list
        } else {
            memberVarList = GetClassMemberVarSizeList(codes, className)
        }

        for varSize in memberVarList {
            total += varSize
        }

        return total
    }
    
    /// Gets the size of each member variables (in bytes) in the specified class
    /// - Parameters:
    ///   - codes: Intermediate codes
    ///   - className: Target class name
    /// - Returns: List of member variable sizes (bytes)
    private static func GetClassMemberVarSizeList(
        _ codes: [String],
        _ className: String
    ) -> [Int] {

        var retList: [Int] = []

        var endClassScopeIndex = -1
        var startClassScopeIndex = -1

        // --------------------------------------------
        // Goto class declaration
        // --------------------------------------------
        for i in 0..<codes.count {

            if codes[i] != PH_RESV_KW_CLASS + className {
                continue
            }

            // Search for the first function, then break
            for j in i..<codes.count {

                let splitFuncName = codes[j].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
                if splitFuncName.isEmpty {
                    continue
                }

                if !AllFunctionKeywords.contains(splitFuncName[0] + " ") {
                    continue
                }

                endClassScopeIndex = j - 1
                break
            }

            // Find class contents start index
            var k = i
            while k < endClassScopeIndex {
                if codes[k] == "{" {
                    startClassScopeIndex = k + 1
                    break
                }
                k += 1
            }

            break
        }

        // --------------------------------------------
        // Extract member variable sizes
        // --------------------------------------------
        if startClassScopeIndex == -1 || endClassScopeIndex == -1 {
            return retList
        }

        for i in startClassScopeIndex..<endClassScopeIndex {

            let varDeclareCode =
                codes[i]
                    .firstMatchString(
                        NSRegularExpression.escapedPattern(for: PH_REG) + "(.*?) "
                    )
                    .trimmingCharacters(in: .whitespacesAndNewlines)

            if varDeclareCode.contains(PH_REG) {
                let bitStr =
                    varDeclareCode
                        .replacingOccurrences(of: PH_REG, with: "")
                        .replacingOccurrences(of: "u", with: "")
                        .replacingOccurrences(of: "s", with: "")

                if let bitSize = Int(bitStr) {
                    let byteSize = bitSize / 8
                    retList.append(byteSize)
                }
            }
        }

        return retList
    }
    
    /// <summary>
    /// Get the bit size from the variable keyword type
    /// </summary>
    /// <param name="varType"></param>
    /// <returns></returns>
    private static func GetBitSizeOfVarDeclaration(_ varType: String) -> Int {
        var bitSize = 0

        switch varType {
        case PH_RESV_KW_PTR:
            bitSize = TARGET_ARCH_BIT_SIZE
        case PH_RESV_KW_VAR:
            bitSize = TARGET_ARCH_BIT_SIZE
        case PH_RESV_KW_OBJECT:
            bitSize = TARGET_ARCH_BIT_SIZE
        case PH_RESV_KW_CHAR:
            bitSize = CHAR_BIT_SIZE
        case PH_RESV_KW_STRING:
            bitSize = TARGET_ARCH_BIT_SIZE
        case PH_RESV_KW_BOOL:
            bitSize = BYTE_BIT_SIZE
        case PH_RESV_KW_SBYTE:
            bitSize = BYTE_BIT_SIZE
        case PH_RESV_KW_SHORT:
            bitSize = SHORT_BIT_SIZE
        case PH_RESV_KW_INT:
            bitSize = INT_BIT_SIZE
        case PH_RESV_KW_LONG:
            bitSize = LONG_BIT_SIZE
        case PH_RESV_KW_BYTE:
            bitSize = BYTE_BIT_SIZE
        case PH_RESV_KW_USHORT:
            bitSize = SHORT_BIT_SIZE
        case PH_RESV_KW_UINT:
            bitSize = INT_BIT_SIZE
        case PH_RESV_KW_ULONG:
            bitSize = LONG_BIT_SIZE
        case PH_RESV_KW_FLOAT:
            bitSize = FLOAT_BIT_SIZE
        case PH_RESV_KW_DOUBLE:
            bitSize = DOUBLE_BIT_SIZE
        case PH_RESV_KW_DECIMAL:
            bitSize = DECIMAL_BIT_SIZE
        default:
            break
        }

        return bitSize
    }
    
    /// <summary>
    /// Converts variables that are class instances to pointer format
    /// (e.g.)
    /// N.INT32 N.Program.Main.a
    /// --->
    /// REGu64 𒀭PH_PTR𒀭N.Program.Main.a
    /// </summary>
    private static func ConvertClassVarDeclarationsToPointers(
        _ codes: [String]
    ) -> [String] {

        var codes = codes

        // Get all classes
        let classDeclarationList =
            GetAllKeywordDeclarations(codes, PH_RESV_KW_CLASS, false)

        // Get all function parameter list within the code
        let declaredFuncParamList =
            GetAllFunctionParameterList(codes)

        // Search entire code
        for i in 0..<codes.count {

            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)
            if splitCode.count <= 1 {
                continue
            }

            // Must be a class type declaration
            if !classDeclarationList.contains(splitCode[0]) {
                continue
            }

            var pointerSymbol = PH_ID + PH_POINTER + PH_ID

            // If this variable is inside a function parameter,
            // we do NOT convert it to a pointer
            if declaredFuncParamList.contains(splitCode[1]) {
                pointerSymbol = ""
            }

            // Replace class declaration with pointer-based REG
            codes[i] =
                ReplaceFirstOccurance(
                    codes[i],
                    splitCode[0] + " ",
                    PH_REG
                        + "s"
                        + String(TARGET_ARCH_BIT_SIZE)
                        + " "
                        + pointerSymbol
                )
        }

        return codes
    }
    
    /// Convert return statement to "RET" ASM code
    private static func ConvertReturnToASM(
        _ codes: [String]
    ) -> [String] {

        var codes = codes
        var i = 0

        while i < codes.count {

            // C# と同じ判定
            if codes[i] + " " != PH_RESV_KW_RETURN {
                i += 1
                continue
            }

            // 安全ガード（Swift用・C#は暗黙に信頼）
            if i + 4 >= codes.count {
                i += 1
                continue
            }

            // [0] return
            // [1] (
            // [2] value
            // [3] )
            // [4] ;
            codes[i]     = ""
            codes[i + 1] = ""
            codes[i + 2] =
                PH_PSH + " " + codes[i + 2] + " " + PH_RET
            codes[i + 3] = ""
            // codes[i + 4] は ";" なのでそのまま（C#も触ってない）

            i += 4
        }

        return codes
    }
    
    /// <summary>
    /// Converts instance's function calls and direct member variable access codes to ASM code
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="instanceObjList"></param>
    /// <param name="declaredVarList"></param>
    /// <returns></returns>
    private static func ConvertInstanceFuncCallAndMemberVarAccessToASM(
        _ codes: [String],
        _ instanceObjList: [String: String],
        _ declaredVarList: [String: String]
    ) -> [String] {
        var codes = codes

        // Get all function list (function name -> return type)
        let funcList = GetAllFunctionList(codes)

        // Build instance member accessible list (obj.)
        var instanceObjMemberAccessibleList: [String: String] = [:]
        for (k, v) in instanceObjList {
            instanceObjMemberAccessibleList[k + "."] = v
        }

        var i = 0
        while i < codes.count {

            let splitCodeRaw =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            var splitCode = splitCodeRaw

            var j = 0
            while j < splitCode.count {

                let objPeriodSplit =
                    splitCode[j].split(separator: ".").map(String.init)

                if objPeriodSplit.count <= 1 {
                    j += 1
                    continue
                }

                // Build object instance name (xxx.)
                var objInstToCheck = ""

                for k in 0..<(objPeriodSplit.count - 1) {
                    let cleaned =
                        objPeriodSplit[k]
                            .replacingOccurrences(
                                of: PH_ID + PH_POINTER + PH_ID,
                                with: ""
                            )
                    objInstToCheck += cleaned + "."
                }

                guard instanceObjMemberAccessibleList[objInstToCheck] != nil else {
                    j += 1
                    continue
                }

                // Remove pointer markers temporarily
                splitCode[j] =
                    splitCode[j].replacingOccurrences(
                        of: PH_ID + PH_POINTER + PH_ID,
                        with: ""
                    )

                let objInstName =
                    String(objInstToCheck.dropLast())

                let memberValue =
                    ReplaceFirstOccurance(
                        splitCode[j],
                        objInstToCheck,
                        instanceObjList[objInstName]! + "."
                    )

                // --------------------------------------------------
                // FUNCTION CALL
                // --------------------------------------------------
                if funcList[memberValue] != nil {

                    splitCode[j] =
                        PH_PSH + " " + objInstName + " " +
                        PH_JMS + " " + instanceObjList[objInstName]! + " " +
                        PH_POP + " " + objInstName + " " +
                        PH_JMS + " " + memberValue

                    if j - 1 >= 0 {
                        splitCode[j - 1] = ""
                    }

                } else {
                    // --------------------------------------------------
                    // MEMBER VARIABLE ACCESS
                    // --------------------------------------------------
                    guard declaredVarList[memberValue] != nil else {
                        j += 1
                        continue
                    }

                    if j - 1 >= 0 {
                        splitCode[j - 1] =
                            PH_PSH + " " + objInstName + " " +
                            PH_JMS + " " + instanceObjList[objInstName]! + " " +
                            PH_POP + " " + objInstName + " " +
                            splitCode[j - 1]
                    }

                    splitCode[j] =
                        PH_ID + PH_POINTER + PH_ID + memberValue
                }

                // Rebuild code line
                var newCode = ""
                for token in splitCode where !token.isEmpty {
                    newCode += token + " "
                }

                codes[i] = newCode
                j += 1
            }

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Converts if else if else statements to ASM code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertIfStatementToASM(
        _ codes: [String]
    ) -> [String] {

        var codes = codes
        var ifBlockScopeEndLabel = ""

        var i = 0
        while i < codes.count {

            // ----------------------------------
            // ELSE block → simply remove
            // ----------------------------------
            if codes[i] == PH_RESV_KW_ELSE {

                let endOfElseBlock =
                    FindEndScopeBlock(codes, i, "{", "}")

                if endOfElseBlock != -1 {
                    codes[i] = ""
                    if i + 1 < codes.count {
                        codes[i + 1] = ""
                    }
                    if endOfElseBlock < codes.count {
                        codes[endOfElseBlock] = ""
                    }
                }

                i += 1
                continue
            }

            // ----------------------------------
            // IF / ELSE IF only
            // ----------------------------------
            if codes[i] != PH_RESV_KW_IF &&
               codes[i] != PH_RESV_KW_ELSEIF {
                i += 1
                continue
            }

            // Reset when new IF starts
            if codes[i] == PH_RESV_KW_IF {
                ifBlockScopeEndLabel = ""
            }

            // ----------------------------------
            // Safety guards (C# assumes valid)
            // ----------------------------------
            guard i + 2 < codes.count,
                  i - 2 >= 0,
                  i - 4 >= 0 else {
                i += 1
                continue
            }

            // Variable already resolved to single value
            let varToEvaluate = codes[i + 2]

            let ifScopeEndLabel =
                codes[i - 2]
                    .replacingOccurrences(
                        of: "_begin_",
                        with: "_end_"
                    )

            if ifBlockScopeEndLabel.isEmpty {
                ifBlockScopeEndLabel =
                    codes[i - 4]
                        .replacingOccurrences(
                            of: "_begin_",
                            with: "_end_"
                        )
            }

            // ----------------------------------
            // Remove: if ( xxx )
            // ----------------------------------
            for j in 0..<4 {
                if i + j < codes.count {
                    codes[i + j] = ""
                }
            }

            let startScopeIndex = i + 4
            let endScopeIndex =
                FindEndScopeBlock(codes, startScopeIndex, "{", "}")

            if endScopeIndex == -1 {
                i += 1
                continue
            }

            // ----------------------------------
            // Generate ASM condition
            // ----------------------------------
            let expression =
                PH_VAR + "u" + String(BYTE_BIT_SIZE) + " " +
                varToEvaluate + " " +
                PH_SKE + " 1 " +
                PH_JMP + " " +
                ifScopeEndLabel + " "

            // ----------------------------------
            // Remove scope brackets
            // ----------------------------------
            if startScopeIndex < codes.count {
                codes[startScopeIndex] = ""
            }

            if endScopeIndex < codes.count {
                codes[endScopeIndex] =
                    PH_JMP + " " + ifBlockScopeEndLabel + " "
            }

            // Insert ASM
            codes[i] = expression

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Convert function parameters to POP ASM codes
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertFunctionParamsToASM(
        _ codes: [String]
    ) -> [String] {

        var codes = codes
        var i = 0

        while i < codes.count {

            let splitCode =
                codes[i].split(separator: " ", omittingEmptySubsequences: false).map(String.init)

            // Must be a function keyword
            if splitCode.isEmpty ||
               !AllFunctionKeywords.contains(splitCode[0] + " ") {
                i += 1
                continue
            }

            // "(" starts right after function declaration
            let startRoundBracketIndex = i + 1
            let endRoundBracketIndex =
                FindEndScopeBlock(
                    codes,
                    startRoundBracketIndex,
                    "(",
                    ")"
                )

            if endRoundBracketIndex == -1 {
                i += 1
                continue
            }

            // ----------------------------------
            // Collect parameters inside ( )
            // ----------------------------------
            var funcParams = ""
            if startRoundBracketIndex + 1 <= endRoundBracketIndex - 1 {
                for j in (startRoundBracketIndex + 1)..<endRoundBracketIndex {
                    funcParams += codes[j]
                }
            }

            // ----------------------------------
            // Remove ( ... )
            // ----------------------------------
            for j in startRoundBracketIndex...(endRoundBracketIndex) {
                if j < codes.count {
                    codes[j] = ""
                }
            }

            // ----------------------------------
            // Generate POP instructions
            // ----------------------------------
            if !funcParams.isEmpty {

                let splitParams =
                    funcParams.split(separator: ",").map(String.init)

                funcParams = ""

                // Reverse order (stack is FILO)
                for param in splitParams.reversed() {

                    let spltParamVar =
                        param.split(separator: " " , omittingEmptySubsequences: false).map(String.init)

                    if spltParamVar.count < 2 {
                        continue
                    }

                    let varType =
                        spltParamVar[0]
                            .replacingOccurrences(
                                of: PH_REG,
                                with: PH_VAR
                            )

                    let varName = spltParamVar[1]

                    funcParams +=
                        param
                        + " "
                        + PH_POP
                        + " "
                        + varName
                        + ";"
                }
            }

            // ----------------------------------
            // Insert POPs right after "{"
            // ----------------------------------
            let insertIndex = endRoundBracketIndex + 2
            if insertIndex < codes.count {
                codes[insertIndex] =
                    funcParams + codes[insertIndex]
            }

            i += 1
        }

        return codes
    }

    /// <summary>
    /// Replace other codes to ASM code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func ReplaceKeywordsToASM(_ inputCodes: [String]) -> [String] {
        var codes = inputCodes

        for i in 0..<codes.count {

            // C#: string[] splitCode = codes[i].Split(' ');
            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count < 1 {
                continue
            }

            // --------------------------------------------------
            // Convert labels to "NOP LABEL_NAME" format
            // --------------------------------------------------
            if splitCode[0].contains(PH_ID + PH_LABEL + PH_ID) {

                codes[i] =
                    PH_NOP + " " +
                    codes[i].replacingOccurrences(
                        of: PH_ID + PH_LABEL + PH_ID,
                        with: ""
                    )
                continue
            }

            // --------------------------------------------------
            // Remove any occurrence of 𒀭PH_LBL𒀭
            // --------------------------------------------------
            codes[i] = codes[i].replacingOccurrences(
                of: PH_ID + PH_LABEL + PH_ID,
                with: ""
            )

            // --------------------------------------------------
            // Convert goto to JMP commands
            // --------------------------------------------------
            if splitCode[0] + " " == PH_RESV_KW_GOTO {
                codes[i] = ReplaceFirstOccurance(
                    codes[i],
                    PH_RESV_KW_GOTO,
                    PH_JMP + " "
                )
            }
        }

        return codes
    }
    
    /// <summary>
    /// Converts classes and functions to labels
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertClassFunctionsToLabels(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        // Convert class to labels
        codes = ConvertKeywordsToLabels(codes, PH_RESV_KW_STATIC_CLASS)
        codes = ConvertKeywordsToLabels(codes, PH_RESV_KW_CLASS)

        // Convert functions to labels
        for keyword in AllFunctionKeywords {
            codes = ConvertKeywordsToLabels(codes, keyword)
        }

        return codes
    }
    
    /// <summary>
    /// Converts given keyword (i.e. "class ", "static_class ", "function_void ", etc) to labels
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    private static func ConvertKeywordsToLabels(
        _ inputCodes: [String],
        _ keyword: String
    ) -> [String] {

        var codes = inputCodes

        var i = 0
        while i < codes.count {

            // C#: codes[i].IndexOf(keyword, 0) != -1
            if !codes[i].contains(keyword) {
                i += 1
                continue
            }

            // ------------------------------------------
            // Find scope bracket start "{"
            // ------------------------------------------
            var scopeBracketStartIndex = i + 1
            var j = i
            while j < codes.count {
                if codes[j] == "{" {
                    scopeBracketStartIndex = j
                    break
                }
                j += 1
            }

            // ------------------------------------------
            // Resolve label name
            // ------------------------------------------
            let derivedClassName =
                ReplaceFirstOccurance(codes[i], keyword, "")

            var labelName = derivedClassName

            // ------------------------------------------
            // Handle class inheritance
            // class A : B
            // ------------------------------------------
            if keyword == PH_RESV_KW_CLASS,
               i + 2 < codes.count,
               codes[i + 1] == ":" {

                labelName += "_" + codes[i + 2]

                // Insert NOP for base class
                codes[i + 1] =
                    PH_NOP + " " + derivedClassName + ";"
                codes[i + 2] = ""

                scopeBracketStartIndex = i + 3
            }

            // ------------------------------------------
            // Replace declaration with label
            // ------------------------------------------
            codes[i] =
                PH_NOP + " " + labelName + ";"

            // ------------------------------------------
            // Find end of scope and close label
            // ------------------------------------------
            let scopeBracketEndIndex =
                FindEndScopeBlock(
                    codes,
                    scopeBracketStartIndex,
                    "{",
                    "}"
                )

            if scopeBracketStartIndex < codes.count {
                codes[scopeBracketStartIndex] = ""
            }

            if scopeBracketEndIndex >= 0,
               scopeBracketEndIndex < codes.count {
                codes[scopeBracketEndIndex] =
                    PH_NOP + " " + labelName + "_end;"
            }

            i += 1
        }

        return codes
    }
    
    /// <summary>
    /// Generate clean ASM code
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func GenerateCleanASMCode(
        _ inputCodes: [String]
    ) -> [String] {

        // ------------------------------------------
        // Merge all codes into one string
        // ------------------------------------------
        var mergedCode = ""
        for code in inputCodes {
            mergedCode += code + " "
        }

        // ------------------------------------------
        // Cleanup (mirror C# behavior)
        // ------------------------------------------
        mergedCode = Cleanup(mergedCode)
        mergedCode = mergedCode.replacingOccurrences(of: ";", with: " ")

        // ------------------------------------------
        // Split into tokens (KEEP empty tokens!)
        // ------------------------------------------
        let splitCode =
            mergedCode
                .split(separator: " ", omittingEmptySubsequences: false)
                .map(String.init)

        // ------------------------------------------
        // Rebuild ASM statements
        // ------------------------------------------
        var newCode = ""
        var i = 0

        while i < splitCode.count {

            let token = splitCode[i]

            if token.isEmpty {
                i += 1
                continue
            }

            // Unary instructions
            if token == PH_RET
                || token == PH_UMN
                || token == PH_UPL
                || token == PH_UNT
                || token == PH_BUC {

                newCode += token + ";"
                i += 1
                continue
            }

            // Binary instructions: OP OPERAND
            if i + 1 < splitCode.count {
                newCode += token + " " + splitCode[i + 1] + ";"
                i += 2
            } else {
                // Safety fallback (should not happen in valid ASM)
                newCode += token + ";"
                i += 1
            }
        }

        // ------------------------------------------
        // Split again by ';' and remove blanks
        // ------------------------------------------
        var codes =
            newCode
                .split(separator: ";", omittingEmptySubsequences: true)
                .map { String($0) }

        // Final trim (mirror C# Where(!IsNullOrWhiteSpace))
        codes =
            codes.filter {
                !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
            }

        return codes
    }
    
    /// <summary>
    /// Convert pointer identifier (PH_ID + PH_POINTER + PH_ID) to "#" symbol
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertPointerKeywordsToSymbol(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        for i in 0..<codes.count {
            codes[i] =
                codes[i].replacingOccurrences(
                    of: PH_ID + PH_POINTER + PH_ID,
                    with: "#"
                )
        }

        return codes
    }
    
    /// <summary>
    /// Adds VM specific codes (like preserved variables)
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func AddVMSpecificCode(
        _ inputCodes: [String]
    ) -> [String] {

        var newCodes = inputCodes

        newCodes.insert(PH_REG + "s" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_SVAR1, at: 0)
        newCodes.insert(PH_REG + "s" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_SVAR0, at: 0)
        newCodes.insert(PH_REG + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR1, at: 0)
        newCodes.insert(PH_REG + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_UVAR0, at: 0)
        newCodes.insert(PH_REG + "u" + String(TARGET_ARCH_BIT_SIZE) + " " + REG_NULL,  at: 0)

        return newCodes
    }
    
    /// <summary>
    /// Optimizes ASM code to remove redundant codes
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func OptimizeRedundantASM(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var i = 0

        while i + 1 < codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count > 1,
               splitCode[0] == PH_PSH {

                let splitNextCode =
                    codes[i + 1]
                        .split(separator: " ", omittingEmptySubsequences: false)
                        .map(String.init)

                if splitNextCode.count > 1,
                   splitNextCode[0] == PH_POP,
                   splitCode[1] == splitNextCode[1] {

                    // Remove redundant PSH / POP
                    codes[i] = ""
                    codes[i + 1] = ""

                    // Skip next line since we consumed it
                    i += 2
                    continue
                }
            }

            i += 1
        }

        // Remove empty lines (mirror C# Where(!IsNullOrWhiteSpace))
        codes = codes.filter {
            !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
        }

        return codes
    }
    
    /// Generate all possible REG{u|s}XXX keywords
    private static func GenerateRegVarKeywords(
        _ keyword: String
    ) -> [String] {

        var allRegVarKeywords: [String] = []

        var i = 8
        while i <= TARGET_ARCH_BIT_SIZE {
            allRegVarKeywords.append(keyword + "u" + String(i))
            allRegVarKeywords.append(keyword + "s" + String(i))
            i *= 2
        }

        return allRegVarKeywords
    }
    
    /// <summary>
    /// Returns the list of variables that are registered with the REG keyword
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="allREGKeywords">List of possible REG keywords</param>
    /// <returns></returns>
    private static func GetRegisteredVarList(
        _ codes: [String],
        _ allREGKeywords: [String]
    ) -> [String] {

        var registeredVarList: [String] = []

        // Get all of the REG{u|s}XXX command line
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count == 1 {
                continue
            }

            if !allREGKeywords.contains(splitCode[0]) {
                continue
            }

            // REG keyword found → store the entire line
            registeredVarList.append(codes[i])
        }

        return registeredVarList
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
    private static func MoveREGVarAndVarInitiationToTopOfCode(
        _ codes: [String],
        _ allREGKeywords: [String],
        _ registeredVarList: [String],
        _ declaredStaticClassConstructorList: [String]
    ) -> [String] {

        var codes = codes
        var newCode: [String] = []

        // --------------------------------------------------
        // Remove REG lines from original code
        // --------------------------------------------------
        for i in 0..<codes.count {
            if registeredVarList.contains(codes[i]) {
                codes[i] = ""
            }
        }

        // --------------------------------------------------
        // Add REG declarations to top
        // --------------------------------------------------
        for s in registeredVarList {
            newCode.append(s)
        }

        // --------------------------------------------------
        // Generate calls to static class constructors
        // --------------------------------------------------
        for s in declaredStaticClassConstructorList {
            newCode.append(PH_JMS + " " + s)
            newCode.append(PH_POP + " " + REG_SVAR0)
        }

        // --------------------------------------------------
        // Add float precision initialization
        // --------------------------------------------------
        newCode.append(PH_PSH + " " + String(Int(DECPOINT_NUM_SUFFIX_F_MULTIPLY_BY)))
        newCode.append(PH_JMS + " " + PH_RESV_SYSFUNC_SET_FLOAT_PRECISION)
        // NOTE: non-returning JMS must POP
        newCode.append(PH_POP + " " + REG_SVAR0)

        // --------------------------------------------------
        // Jump to init label
        // --------------------------------------------------
        newCode.append(PH_JMS + " " + "@DScript_Init")
        newCode.append(PH_POP + " " + REG_SVAR0)

        // --------------------------------------------------
        // Jump to END after init
        // --------------------------------------------------
        newCode.append(PH_JMP + " " + "@END")

        // --------------------------------------------------
        // Append remaining original code
        // --------------------------------------------------
        for code in codes {
            if code.isEmpty { continue }
            newCode.append(code)
        }

        // --------------------------------------------------
        // Add END label
        // --------------------------------------------------
        newCode.append(PH_NOP + " " + "@END")

        // --------------------------------------------------
        // Insert VM entry stub (ignored on first run)
        // --------------------------------------------------
        newCode.insert(PH_JMP + " " + "@END", at: 0)
        newCode.insert(PH_POP + " " + REG_SVAR0, at: 0)
        newCode.insert(PH_JMS + " " + "@DScript_Main", at: 0)

        return newCode
    }
    
    /// <summary>
    /// Converts REG to VAR
    /// REG{u|v}XXX will be converted to VAR{u|v}XXX ---> The VM will register the first appearing variables as new variables automatically,
    /// Hence we won't need to distinguish between REG and VAR
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertRegToVarCommand(_ codes: [String]) -> [String] {
        var codes = codes

        for i in 0..<codes.count {
            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count <= 1 {
                continue
            }

            let newHead =
                splitCode[0].replacingOccurrences(of: PH_REG, with: PH_VAR)

            codes[i] = newHead + " " + splitCode[1]
        }

        return codes
    }

    /// <summary>
    /// Remove all labels and convert label references on JMP and JMS to absolute program code index
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func RemoveLabelsAndConvertJumpToCodeIndex(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes
        var labelList: [String: String] = [:]

        var reducedLineOfCode = 0

        // --------------------------------------------------
        // Pass 1: collect labels & remove them
        // --------------------------------------------------
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.isEmpty { continue }
            if splitCode[0] != PH_NOP { continue }
            if splitCode.count < 2 { continue }

            let codeIndex = i - reducedLineOfCode
            reducedLineOfCode += 1

            // label name -> absolute code index
            labelList[splitCode[1]] = String(codeIndex)

            // remove label line
            codes[i] = ""
        }

        // --------------------------------------------------
        // Pass 2: replace JMP / JMS label with code index
        // --------------------------------------------------
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count < 2 { continue }

            if splitCode[0] == PH_JMP || splitCode[0] == PH_JMS {

                let labelName = splitCode[1]

                if let targetIndex = labelList[labelName] {
                    codes[i] = splitCode[0] + " " + targetIndex
                }
            }
        }

        // --------------------------------------------------
        // Remove empty lines
        // --------------------------------------------------
        codes = codes.filter {
            !$0.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
        }

        // --------------------------------------------------
        // Add extra line for @END label
        // --------------------------------------------------
        codes.append("")

        return codes
    }
    
    /// <summary>
    /// Convert all variable names that are declared with REG and VAR to numeric indexes with preceeding "$" symbol
    /// If a variable is declared as pointer, or referencing a pointer, it is added with a preceeding "#" symbol instead
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="registeredVarList"></param>
    /// <returns></returns>
    private static func ConvertVariablesToID(
        _ inputCodes: [String],
        _ registeredVarList: [String]
    ) -> [String] {

        var codes = inputCodes

        // varName -> "$ID"
        var regVarIndexList: [String: String] = [:]

        // --------------------------------------------------
        // 1. Assign numeric IDs to registered variables
        // --------------------------------------------------
        // NOTE: first 3 lines are JMS bootstrap codes, skip them
        for i in 3..<(registeredVarList.count + 3) {

            let varID = i - 3
            let splitVar =
                registeredVarList[varID]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitVar.count < 2 { continue }

            let newVarType = splitVar[0].replacingOccurrences(of: PH_REG, with: PH_VAR)

            // C# currently always uses "$"
            let varAddressRefSymbol = "$"

            let varName = splitVar[1].replacingOccurrences(of: "#", with: "")
            let varIDString = varAddressRefSymbol + String(varID)

            // Register mapping
            regVarIndexList[varName] = varIDString

            // Rewrite declaration line itself
            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count >= 2 {
                codes[i] = newVarType + " " + varIDString
            }
        }

        // --------------------------------------------------
        // 2. Replace all variable references with numeric IDs
        // --------------------------------------------------
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.count < 2 { continue }

            let rawVarName = splitCode[1]
            let varToCheck = rawVarName.replacingOccurrences(of: "#", with: "")

            guard let mappedID = regVarIndexList[varToCheck] else {
                continue
            }

            // Replace variable name with ID
            codes[i] = codes[i].replacingOccurrences(of: varToCheck, with: mappedID)

            // If original was pointer reference, remove "$"
            if rawVarName.contains("#") {
                codes[i] = codes[i].replacingOccurrences(of: "$", with: "")
            }
        }

        // --------------------------------------------------
        // 3. Fix static pointer variables (declared with "#")
        // --------------------------------------------------
        // Again, skip first 3 bootstrap lines
        for i in 3..<(registeredVarList.count + 3) {

            let varID = i - 3

            if !registeredVarList[varID].contains("#") {
                continue
            }

            // Replace "$" with "#"
            codes[i] = codes[i].replacingOccurrences(of: "$", with: "#")
        }

        return codes
    }

    /// <summary>
    /// Convert all labels to numeric IDs, and the labels referenced by JMP and JMS to the equivalent ID
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertJumpAndLabelsToNumericID(
        _ inputCodes: [String]
    ) -> [String] {

        var codes = inputCodes

        // labelName -> numeric ID
        var labelList: [String: Int] = [:]
        var labelID = 0

        // --------------------------------------------------
        // 1. Collect labels and replace NOP label with NOP <id>
        // --------------------------------------------------
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.isEmpty { continue }
            if splitCode[0] != PH_NOP { continue }
            if splitCode.count < 2 { continue }

            let labelName = splitCode[1]
            labelList[labelName] = labelID

            codes[i] = PH_NOP + " " + String(labelID)
            labelID += 1
        }

        // --------------------------------------------------
        // 2. Replace JMP / JMS label references with numeric IDs
        // --------------------------------------------------
        for i in 0..<codes.count {

            let splitCode =
                codes[i]
                    .split(separator: " ", omittingEmptySubsequences: false)
                    .map(String.init)

            if splitCode.isEmpty { continue }

            if splitCode[0] != PH_JMP && splitCode[0] != PH_JMS {
                continue
            }

            if splitCode.count < 2 { continue }

            let labelName = splitCode[1]

            if let id = labelList[labelName] {
                codes[i] = splitCode[0] + " " + String(id)
            }
        }

        return codes
    }
    
    // -------------------------------------------------------
    // CONVERT VIRTUAL ASSEMBLY LANGUAGE TO BYTE CODE
    // -------------------------------------------------------
    
    /// <summary>
    /// Converts all ASM codes to byte codes
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    private static func ConvertAsmToByteCode(_ codes: [String]) -> [UInt8] {
        var binCode: [UInt8] = []

        // (variable name -> type) 例: "$0" -> "u32"
        var varList: [String: String] = [:]

        // ------------------------------------
        // ① Collect VAR declarations
        // ------------------------------------
        // NOTE: First 3 lines are JMS jump to main
        for i in 3..<codes.count {
            let splitCode = codes[i].split(separator: " ", omittingEmptySubsequences: false)
                .map(String.init)

            if splitCode.isEmpty { continue }

            // Stop when VAR declaration ends
            if !splitCode[0].contains(PH_VAR) {
                break
            }

            // e.g. VARu32 $0
            let varType = splitCode[0].replacingOccurrences(of: PH_VAR, with: "")
            let varName = splitCode[1].replacingOccurrences(of: "#", with: "$")

            varList[varName] = varType
        }

        // ------------------------------------
        // ② Convert each ASM line to byte code
        // ------------------------------------
        for line in codes {
            let splitCode = line.split(separator: " ", omittingEmptySubsequences: false)
                .map(String.init)

            let cmd = splitCode.first ?? ""
            let val = splitCode.count > 1 ? splitCode[1] : ""

            let byteCode =
                ConvertSingleLineOfCodeToByteCode(cmd, val, varList)

            binCode.append(contentsOf: byteCode)
        }
        
        // Test
        if IsGenerateUnitTestFiles { TestConvertSingleLineOfCodeToByteCode = binCode }

        // ------------------------------------
        // ③ Remove labels and fix JMP/JMS targets
        // ------------------------------------
        var newByteCodes = binCode
        newByteCodes =
            RemoveLabelIDsAndConvertJumpToBinCodeIndex(newByteCodes)

        // Test
        if IsGenerateUnitTestFiles { TestRemoveLabelIDsAndConvertJumpToBinCodeIndex = newByteCodes }
        
        return newByteCodes
    }
    
    /// <summary>
    /// Convert one line of IASM code to byte code
    /// NOTE: The value will be converted to the target architecture maximum size (i.e. 64 bit = 8 bytes)
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="varList"></param>
    /// <returns></returns>
    private static func ConvertSingleLineOfCodeToByteCode(
        _ cmd: String,
        _ value: String,
        _ varList: [String: String]
    ) -> [UInt8] {

        var byteCodes: [UInt8] = []

        var cmdByteCode: UInt8 = 0
        var valType: UInt8 = TYPE_NULL
        var byteValue: [UInt8] = []

        var valueStr = value

        // --------------------------------------------------
        // Determine value type
        // --------------------------------------------------
        if !valueStr.isEmpty {

            if valueStr.first == "$" {
                // Variable
                let varName = valueStr.replacingOccurrences(of: "$", with: "")
                if let varType = varList["$" + varName] {

                    switch varType {
                    case "u8":  valType = TYPE_UVAR8
                    case "s8":  valType = TYPE_SVAR8
                    case "u16": valType = TYPE_UVAR16
                    case "s16": valType = TYPE_SVAR16
                    case "u32": valType = TYPE_UVAR32
                    case "s32": valType = TYPE_SVAR32
                    case "u64": valType = TYPE_UVAR64
                    case "s64": valType = TYPE_SVAR64
                    default: break
                    }
                }

                valueStr = varName

            } else if valueStr.first == "#" {
                // Pointer reference
                valType = TYPE_PTR
                valueStr = valueStr.replacingOccurrences(of: "#", with: "")
            } else {
                // Immediate value
                valType = TYPE_IMM
            }

        } else {
            valType = TYPE_NULL
        }

        // --------------------------------------------------
        // Determine command byte
        // --------------------------------------------------
        if cmd.contains(PH_VAR) {
            cmdByteCode = VAR
        }

        switch cmd {
        case PH_NOP: cmdByteCode = NOP
        case PH_VMC: cmdByteCode = VMC
        case PH_MOV: cmdByteCode = MOV
        case PH_JMP: cmdByteCode = JMP
        case PH_JMS: cmdByteCode = JMS
        case PH_RET: cmdByteCode = RET
        case PH_ADD: cmdByteCode = ADD
        case PH_SUB: cmdByteCode = SUB
        case PH_MLT: cmdByteCode = MLT
        case PH_DIV: cmdByteCode = DIV
        case PH_MOD: cmdByteCode = MOD
        case PH_SHL: cmdByteCode = SHL
        case PH_SHR: cmdByteCode = SHR
        case PH_CAN: cmdByteCode = CAN
        case PH_COR: cmdByteCode = COR
        case PH_SKE: cmdByteCode = SKE
        case PH_AND: cmdByteCode = AND
        case PH_LOR: cmdByteCode = LOR
        case PH_XOR: cmdByteCode = XOR
        case PH_PSH: cmdByteCode = PSH
        case PH_POP: cmdByteCode = POP
        case PH_LEQ: cmdByteCode = LEQ
        case PH_LNE: cmdByteCode = LNE
        case PH_LSE: cmdByteCode = LSE
        case PH_LGE: cmdByteCode = LGE
        case PH_LGS: cmdByteCode = LGS
        case PH_LGG: cmdByteCode = LGG
        case PH_UMN: cmdByteCode = UMN
        case PH_UPL: cmdByteCode = UPL
        case PH_UNT: cmdByteCode = UNT
        case PH_BUC: cmdByteCode = BUC
        default: break
        }

        // --------------------------------------------------
        // Emit opcode + value type
        // --------------------------------------------------
        byteCodes.append(cmdByteCode)
        byteCodes.append(valType)

        // --------------------------------------------------
        // Emit value bytes (if any)
        // --------------------------------------------------
        if valType != TYPE_NULL {

            switch TARGET_ARCH_BIT_SIZE {
            case 64:
                if let v = Int64(valueStr) {
                    byteValue = withUnsafeBytes(of: v.littleEndian, Array.init)
                }
            case 32:
                if let v = Int32(valueStr) {
                    byteValue = withUnsafeBytes(of: v.littleEndian, Array.init)
                }
            case 16:
                if let v = Int16(valueStr) {
                    byteValue = withUnsafeBytes(of: v.littleEndian, Array.init)
                }
            default:
                break
            }

            for b in byteValue.reversed() {
                byteCodes.append(b)
            }
        }

        return byteCodes
    }
    
    /// <summary>
    /// Remove all labels and convert label ID references on JMP and JMS to absolute program code index
    /// </summary>
    /// <param name="codes"></param>
    /// <returns></returns>
    public static func RemoveLabelIDsAndConvertJumpToBinCodeIndex(
        _ inputCodes: [UInt8]
    ) -> [UInt8] {

        // labelID -> code index
        var labelList: [Int: Int] = [:]
        var codeList = inputCodes

        // --------------------------------------------
        // Pass 1: collect labels & remove NOP labels
        // --------------------------------------------
        var i = 0
        while i < codeList.count {

            // for(i++) の再現
            defer { i += 1 }

            let cmd  = codeList[i]
            let type = codeList[i + 1]

            if cmd != NOP {

                i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize

                // RET / unary ops have no value payload
                if cmd == RET || cmd == UMN || cmd == UPL || cmd == UNT || cmd == BUC {
                    i -= MaxOpcodeValueSize
                }

                // C# の i--
                i -= 1
                continue
            }

            if type == TYPE_NULL {
                break
            }

            // ラベルIDを取得
            let labelID =
                Int(
                    GetBinCodeValue(
                        codeList,
                        i + MAX_OPCODE_BYTE_SIZE
                    )
                )

            labelList[labelID] = i

            // この NOP ラベル命令自体を削除
            let removeCount = MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize
            codeList.removeSubrange(i..<(i + removeCount))

            // C# の i--
            i -= 1
        }

        var codes = codeList

        // --------------------------------------------
        // Pass 2: rewrite JMP / JMS targets
        // --------------------------------------------
        i = 0
        while i < codes.count {

            // for(i++) の再現
            defer { i += 1 }

            let cmd  = codes[i]
            let type = codes[i + 1]

            if cmd != JMP && cmd != JMS {

                i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize

                if cmd == RET || cmd == UMN || cmd == UPL || cmd == UNT || cmd == BUC {
                    i -= MaxOpcodeValueSize
                }

                // C# の i--
                i -= 1
                continue
            }

            let labelID =
                Int(
                    GetBinCodeValue(
                        codes,
                        i + MAX_OPCODE_BYTE_SIZE
                    )
                )

            guard let targetAddr = labelList[labelID] else {
                continue
            }

            // Big Endian でアドレスを書き換え
            let addrBytes =
                withUnsafeBytes(
                    of: Int64(targetAddr).bigEndian,
                    Array.init
                )

            for j in 0..<MaxOpcodeValueSize {
                codes[i + 2 + j] =
                    addrBytes[addrBytes.count - MaxOpcodeValueSize + j]
            }

            i += MAX_OPCODE_BYTE_SIZE + MaxOpcodeValueSize
            i -= 1
        }

        return codes
    }
    
    /// <summary>
    /// Get the actual value (in long size) in the BinCode
    /// The value size (in bytes) is dependant on the specified MaxBitSize
    /// </summary>
    /// <param name="byteCodeList">Reference to the byte list</param>
    /// <param name="startBinCodeIndex">Start address to seek from</param>
    /// <returns></returns>
    private static func GetBinCodeValue(
        _ codes: [UInt8],
        _ startBinCodeIndex: Int
    ) -> Int64 {

        var value: Int64 = 0
        let byteSize = TARGET_ARCH_BIT_SIZE / 8

        for i in 0..<byteSize {
            value <<= 8
            value |= Int64(codes[startBinCodeIndex + i])
        }

        return value
    }
}

// MARK: - Regex helpers
private extension String {
    /// Returns the first regex match string for the given pattern, or an empty string.
    func firstMatchString(_ pattern: String) -> String {
        guard let regex = try? NSRegularExpression(pattern: pattern, options: [.dotMatchesLineSeparators]) else {
            return ""
        }
        let ns = self as NSString
        let range = NSRange(location: 0, length: ns.length)
        guard let match = regex.firstMatch(in: self, options: [], range: range) else {
            return ""
        }
        return ns.substring(with: match.range)
    }
}

