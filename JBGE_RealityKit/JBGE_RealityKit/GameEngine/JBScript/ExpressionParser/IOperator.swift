//
//  IOperator.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

// IOperator extends Token
public protocol IOperator: Token {
    var AsString: String { get }

    /// <summary>
    /// Operator precedence.
    /// </summary>
    var Precedence: Int { get }

    var `Type`: OperatorType { get }
    var ShortCircuit: ShortCircuitMode { get }
}

public enum ShortCircuitMode: Int {
    case None = 0
    case LogicalAnd
    case LogicalOr
}

public enum OperatorType: Int {
    case Operator
    case UnaryOperator
    case Function
    case OpenParenthesis
    case CloseParenthesis
    case UnaryCastOperator
}
