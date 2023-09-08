using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public class JointFunctionSolver {
    static string operators = "+-*/%";
    static Dictionary<string, Func<float, float>> singalArgMathFunc = new Dictionary<string, Func<float, float>> {
        { "neg"   , x => -x},
        { "rad"   , x => x*Mathf.Deg2Rad},
        { "deg"   , x => x*Mathf.Rad2Deg},
        { "sin"   , Mathf.Sin},
        { "asin"  , Mathf.Asin},
        { "cos"   , Mathf.Cos},
        { "acos"  , Mathf.Acos},
        { "tan"   , Mathf.Tan},
        { "atan"  , Mathf.Atan},
        { "sqrt"  , Mathf.Sqrt},
        { "ln"    , Mathf.Log},
        { "exp"   , Mathf.Exp},
        { "abs"   , Mathf.Abs},
        { "floor" , Mathf.Floor},
        { "ceil"  , Mathf.Ceil},
        { "int"   , Mathf.Floor},
        { "sgn"   , Mathf.Sign},
    };
    static Dictionary<string, Func<float, float, float>> doubleArgMathFunc = new Dictionary<string, Func<float, float, float>> {
        { "atan2" , Mathf.Atan2},
        { "pow" , Mathf.Pow},
    };

    static HashSet<string> refrenceFunction = new HashSet<string> {
        {"T"},
        {"D"},
    };
    static public float Solve(Dictionary<string, KinematicJoint> kinematicJointList, List<string> postFix) {
        Stack<float> valStack = new Stack<float>();
        for (int i = 0; i < postFix.Count; ++i) {
            var arg = postFix[i];
            if (float.TryParse(arg, out var val)) { //number
                valStack.Push(val);
            } else if (operators.Contains(arg)) { //+-*/%
                var y = valStack.Pop();
                var x = valStack.Pop();
                Func<float> valueFunc = () => {
                         if (arg == "+") return x + y;
                    else if (arg == "-") return x - y;
                    else if (arg == "*") return x * y;
                    else if (arg == "/") return x / y;
                    else if (arg == "%") return x % y;
                    else throw new NotImplementedException();
                };
                valStack.Push(valueFunc());
            } else if(singalArgMathFunc.ContainsKey(arg)) {
                var x = valStack.Pop();
                valStack.Push(singalArgMathFunc[arg](x));
            } else if (doubleArgMathFunc.ContainsKey(arg)) {
                var y = valStack.Pop();
                var x = valStack.Pop();
                valStack.Push(doubleArgMathFunc[arg](x, y));
            } else {
                var jointName = arg;
                ++i;
                var refFun = postFix[i];
                valStack.Push(kinematicJointList[jointName].userValue);
            }
        }
        if(valStack.Count != 1) {
            throw new Exception("Bad caculation.");
        }
        var res = valStack.Pop();
        
        return res;
    }

    static int GetPrecedence(string _operator) {
        if ("+-".Contains(_operator))
            return 0;
        if ("*/%".Contains(_operator))
            return 1;
        return 2;
    }

    public static List<string> ConvertToPostfix(string fun) {
        if (string.IsNullOrWhiteSpace(fun))
            return null;

        fun = fun.Replace(" ", "");
        fun = Regex.Replace(fun, @"\)([a-zA-Z\(])", ")*$1");
        fun = fun.Replace("(-", "neg(");

        List<string> splitResult = new List<string>(Regex.Split(fun, @"([\(\)\+\-\*/%,])"));
        splitResult.RemoveAll(s => string.IsNullOrWhiteSpace(s) || s == ",");

        var res = new List<string>();
        Stack<string> stack = new Stack<string>();
        foreach (var c in splitResult) {
            if (c == "(") {
                stack.Push(c);
            } else if (c == ")") {
                for (; stack.Peek() != "("; res.Add(stack.Pop())) ;
                stack.Pop();
            } else if (operators.Contains(c)) { //operator 
                for (; stack.Count != 0 && stack.Peek() != "(" && GetPrecedence(c) <= GetPrecedence(stack.Peek()); res.Add(stack.Pop())) ;
                stack.Push(c);
            } else if (singalArgMathFunc.ContainsKey(c) || doubleArgMathFunc.ContainsKey(c) || refrenceFunction.Contains(c)) { // math function                                   //for (; stack.Count != 0 && stack.Peek() != "("; res.Add(stack.Pop())) ;
                stack.Push(c);
            }else if (float.TryParse(c, out var value)) { //number
                res.Add(c);
            } else { //joitn name
                res.Add(c);
            }
        }
        for (; stack.Count != 0; res.Add(stack.Pop())) ;
        return res;
    }

}

}