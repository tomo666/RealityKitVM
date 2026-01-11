//
//  TokenService.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class TokenService {

    public static func TokensAsString(_ tokens: [Token]) -> String {
        var sb = ""

        for token in tokens {
            if let operand = token as? IOperand {
                sb.append("(\(operand.`Type`):\(String(describing: operand.GetValue()))) ")
            } else {
                // Operator
                sb.append("[\(String(describing: token))] ")
            }
        }

        return sb
    }
}
