//
//  Operand.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class Operand: IOperand, Token {

    private static let Bogey: Int64 = -1

    // MARK: - Initializers

    public convenience init(_ operandType: OperandType, _ operandValue: Any?) {
        self.init(Self.Bogey, operandType, operandValue)
    }

    internal init(_ operandType: OperandType) {
        self.Type = operandType
        self.OperandValue = nil
        self.ParserPosition = -1
    }

    public init(_ parserPosition: Int64, _ operandType: OperandType, _ operandValue: Any?) {
        assert(Self.CheckValueType(operandType, operandValue) == true)

        self.Type = operandType
        self.OperandValue = operandValue
        self.ParserPosition = parserPosition
    }

    // MARK: - Properties

    public let ParserPosition: Int64
    public let `Type`: OperandType
    private let OperandValue: Any?

    public var IsNumber: Bool {
        return (`Type` == .Double) || (`Type` == .Long)
    }

    public var TokenType: TokenType {
        .Operand
    }

    // MARK: - Value access

    public func GetValue() -> Any? {
        assert(Self.CheckValueType(Type, OperandValue) == true)
        return OperandValue
    }

    // MARK: - Type checking (legacy, kept for parity)

    private static func OldCheckValueType(_ tokenType: OperandType, _ tokenValue: Any?) -> Bool {
        switch tokenType {
        case .Long:
            return tokenValue is Int64
        case .NullableLong:
            return tokenValue == nil || tokenValue is Int64
        case .Double:
            return tokenValue is Double
        case .NullableDouble:
            return tokenValue == nil || tokenValue is Double
        case .String:
            return tokenValue is String
        case .Variable:
            return tokenValue is String
        case .Bool:
            return tokenValue is Bool
        case .NullableBool:
            return tokenValue == nil || tokenValue is Bool
        case .Object:
            return true
        case .Null:
            return tokenValue == nil
        default:
            return false
        }
    }

    // MARK: - Type checking (current)

    private static func CheckValueType(_ tokenType: OperandType, _ tokenValue: Any?) -> Bool {
        switch tokenType {
        case .Sbyte:
            return tokenValue is Int8
        case .Byte:
            return tokenValue is UInt8
        case .Short:
            return tokenValue is Int16
        case .Ushort:
            return tokenValue is UInt16
        case .Int:
            return tokenValue is Int32
        case .Uint:
            return tokenValue is UInt32
        case .Long:
            return tokenValue is Int64
        case .Ulong:
            return tokenValue is UInt64
        case .Char:
            return tokenValue is Character
        case .Float:
            return tokenValue is Float
        case .Double:
            return tokenValue is Double
        case .Bool:
            return tokenValue is Bool
        case .Decimal:
            return tokenValue is Decimal

        case .NullableSbyte:
            return tokenValue == nil || tokenValue is Int8
        case .NullableByte:
            return tokenValue == nil || tokenValue is UInt8
        case .NullableShort:
            return tokenValue == nil || tokenValue is Int16
        case .NullableUshort:
            return tokenValue == nil || tokenValue is UInt16
        case .NullableInt:
            return tokenValue == nil || tokenValue is Int32
        case .NullableUint:
            return tokenValue == nil || tokenValue is UInt32
        case .NullableLong:
            return tokenValue == nil || tokenValue is Int64
        case .NullableUlong:
            return tokenValue == nil || tokenValue is UInt64
        case .NullableChar:
            return tokenValue == nil || tokenValue is Character
        case .NullableFloat:
            return tokenValue == nil || tokenValue is Float
        case .NullableDouble:
            return tokenValue == nil || tokenValue is Double
        case .NullableBool:
            return tokenValue == nil || tokenValue is Bool
        case .NullableDecimal:
            return tokenValue == nil || tokenValue is Decimal

        case .String:
            return tokenValue == nil || tokenValue is String
        case .Variable:
            return tokenValue is String
        case .Object:
            return true
        case .Null:
            return tokenValue == nil
        case .VSet:
            return tokenValue == nil
        }
    }

    public func ToString() -> String {
        if let value = OperandValue {
            return String(describing: value)
        }
        return "null"
    }
}
