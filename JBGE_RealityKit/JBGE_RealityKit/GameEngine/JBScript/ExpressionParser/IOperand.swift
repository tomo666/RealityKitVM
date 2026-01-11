//
//  IOperand.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public protocol IOperand: Token {
    func GetValue() -> Any?
    var IsNumber: Bool { get }
    var `Type`: OperandType { get }
}

public enum OperandType: Int {
    case Sbyte
    case Byte
    case Short
    case Ushort
    case Int
    case Uint
    case Long
    case Ulong
    case Char
    case Float
    case Double
    case Bool
    case Decimal
    case NullableSbyte
    case NullableByte
    case NullableShort
    case NullableUshort
    case NullableInt
    case NullableUint
    case NullableLong
    case NullableUlong
    case NullableChar
    case NullableFloat
    case NullableDouble
    case NullableBool
    case NullableDecimal
    case String
    case Variable
    case VSet
    case Object
    case Null
}
