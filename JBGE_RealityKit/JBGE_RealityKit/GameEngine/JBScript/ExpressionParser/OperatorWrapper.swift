//
//  OperatorWrapper.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class OperatorWrapper: IOperator {

    public let WrappedOperator: IOperator
    public let ParserPosition: Int64

    public init(_ op: IOperator, _ parserPosition: Int64) {
        self.WrappedOperator = op
        self.ParserPosition = parserPosition
    }

    // MARK: - Token

    public var TokenType: TokenType {
        WrappedOperator.TokenType
    }

    // MARK: - IOperator passthrough

    public var AsString: String {
        WrappedOperator.AsString
    }

    public var Precedence: Int {
        WrappedOperator.Precedence
    }

    public var `Type`: OperatorType {
        WrappedOperator.`Type`
    }

    public var ShortCircuit: ShortCircuitMode {
        WrappedOperator.ShortCircuit
    }

    public func ToString() -> String {
        return WrappedOperator.ToString()
    }
}
