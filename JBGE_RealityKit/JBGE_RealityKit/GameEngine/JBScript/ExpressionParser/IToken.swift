//
//  IToken.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public protocol Token {
    var TokenType: TokenType { get }
    func ToString() -> String
}

public enum TokenType: Int {
    case Operator = 0
    case Operand
}
