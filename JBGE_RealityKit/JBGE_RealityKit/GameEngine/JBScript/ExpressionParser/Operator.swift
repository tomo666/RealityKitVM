//
//  Operator.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class Operator: IOperator {

    private var _precedence: Int

    public var IsOperator: Bool { true }
    public var IsOperand: Bool { false }

    public init(
        _ operatorType: OperatorType,
        _ precedence: Int,
        _ shortCircuit: ShortCircuitMode,
        _ asString: String
    ) {
        self._precedence = precedence
        self.ShortCircuit = shortCircuit
        self.AsString = asString
        self.`Type` = operatorType
    }

    // MARK: - IOperator

    public var Precedence: Int {
        get {
            // Same behavior as C# version
            return _precedence
        }
        set {
            _precedence = newValue
        }
    }

    public let ShortCircuit: ShortCircuitMode
    public let `Type`: OperatorType

    // MARK: - Token

    public var TokenType: TokenType {
        .Operator
    }

    public let AsString: String


    public func ToString() -> String {
        return AsString
    }
}
