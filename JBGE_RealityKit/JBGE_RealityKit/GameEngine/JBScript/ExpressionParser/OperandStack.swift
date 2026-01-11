//
//  OperandStack.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class OperandStack {

    private var _stack: [IOperand] = []

    public init() {}

    // MARK: - Stack operations

    public func Push(_ operand: IOperand) {
        _stack.append(operand)
    }

    @discardableResult
    public func Pop() -> IOperand? {
        return _stack.popLast()
    }

    public func Peek() -> IOperand? {
        return _stack.last
    }

    public var Count: Int {
        return _stack.count
    }

    public var IsEmpty: Bool {
        return _stack.isEmpty
    }

    // MARK: - Debug output

    public func ToShortString() -> String {
        return TokenService.TokensAsString(_stack)
    }

    public var description: String {
        return TokenService.TokensAsString(_stack)
    }
}
