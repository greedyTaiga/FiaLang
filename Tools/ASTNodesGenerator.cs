using System;
using System.Text;
using Fia;

namespace Fia.Tools
{
    internal class ASTNodesGenerator
    {
        public static void main(string[] args)
        {
            var subclasses = new List<string>();

            //Generate the expression classes
            subclasses.Add("Unary       : Token oper, Expr right");
            subclasses.Add("Binary      : Expr left, Token oper, Expr right");
            subclasses.Add("Grouping    : Expr expr");
            subclasses.Add("Literal     : Object? value");
            subclasses.Add("Variable    : Token name");
            subclasses.Add("Assigment   : Token name, Expr value");
            subclasses.Add("Logical     : Expr left, Token oper, Expr right");
            subclasses.Add("Call        : Expr callee, Token paren, List<Expr> arguments");

            GenerateClass("C:/Users/alger/Desktop/Projects/C#/Fia/Fia/Expr.cs",
                "Expr", subclasses);

            subclasses.Clear();

            //Generate the statement classes
            subclasses.Add
                ("Block       : List<Stmt> statements"); 
            subclasses.Add
                ("Expression  : Expr expr");
            subclasses.Add
                ("Print       : Expr val");
            subclasses.Add
                ("Var         : Token name, Expr? init"); 
            subclasses.Add
                ("Loop        : Expr condition, Stmt stmt");
            subclasses.Add
                ("Conditional : Expr condition, Stmt thenBranch, Stmt? elseBranch");
            subclasses.Add
                ("Function    : Token name, List<Token> parameters, List<Stmt> body");
            subclasses.Add
                ("Returning   : Token keyword, Expr? value");

            GenerateClass("C:/Users/alger/Desktop/Projects/C#/Fia/Fia/Stmt.cs",
                "Stmt", subclasses);

        }

        //generates abstact class "superClass", and inner classes from subclasses list
        //subclasses inherit superClass, but are also inside it
        //subclasses list has a structure of "Subclass: FieldType fieldName, ..."
        private static void GenerateClass(string path, string superClass,
            List<string> subclasses)
        {
            var code = new StringBuilder();

            code.Append("namespace Fia {\n");
            code.Append($"abstract class {superClass} {{\n");
            code.Append("public abstract R Accept<R>(IVisitor<R> visitor);");

            GenerateVisitor(superClass, subclasses, code);

            foreach (var subclass in subclasses)
            {
                GenerateSubclass(superClass, code, subclass);
            }

            code.Append("}\n}");

            File.WriteAllText(path, code.ToString());
        }

        private static void GenerateVisitor(string superClass, List<string> subclasses, StringBuilder code)
        {

            code.Append("   public interface IVisitor<R>\n{\n");
            foreach (string subclass in subclasses)
            {
                string subclassName = subclass.Split(":")[0].Trim();
                code.Append($"      public R Visit{subclassName}" +
                    $"({superClass}.{subclassName} {subclassName.ToLower()});\n");
            }

            code.Append("}\n");
        }

        private static void GenerateSubclass(string superClass, StringBuilder code,
            string subclass)
        {
            var name = subclass.Split(":")[0].Trim();
            var fields = subclass.Split(":")[1].Trim();

            code.Append($"public class {name} : {superClass} {{\n");

            foreach (var field in fields.Split(","))
            {
                code.Append($"public readonly {field.Trim()};\n");
            }

            code.Append($"public {name} ({fields}) {{\n");

            foreach (var field in fields.Split(","))
            {
                var fieldName = field.Trim().Split(" ")[1];
                code.Append($"this.{fieldName} = {fieldName};\n");
            }
            code.Append("}\n");
            code.Append($"public override R Accept<R>(IVisitor<R> visitor)" +
                $"{{\nreturn visitor.Visit{name}(this);}}\n");
            code.Append("}\n");
        }

    }
}


