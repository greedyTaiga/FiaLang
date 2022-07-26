namespace Fia {
abstract class Stmt {
public abstract R Accept<R>(IVisitor<R> visitor);   public interface IVisitor<R>
{
      public R VisitBlock(Stmt.Block block);
      public R VisitExpression(Stmt.Expression expression);
      public R VisitPrint(Stmt.Print print);
      public R VisitVar(Stmt.Var var);
      public R VisitLoop(Stmt.Loop loop);
      public R VisitConditional(Stmt.Conditional conditional);
      public R VisitFunction(Stmt.Function function);
      public R VisitReturning(Stmt.Returning returning);
}
public class Block : Stmt {
public readonly List<Stmt> statements;
public Block (List<Stmt> statements) {
this.statements = statements;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitBlock(this);}
}
public class Expression : Stmt {
public readonly Expr expr;
public Expression (Expr expr) {
this.expr = expr;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitExpression(this);}
}
public class Print : Stmt {
public readonly Expr val;
public Print (Expr val) {
this.val = val;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitPrint(this);}
}
public class Var : Stmt {
public readonly Token name;
public readonly Expr? init;
public Var (Token name, Expr? init) {
this.name = name;
this.init = init;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitVar(this);}
}
public class Loop : Stmt {
public readonly Expr condition;
public readonly Stmt stmt;
public Loop (Expr condition, Stmt stmt) {
this.condition = condition;
this.stmt = stmt;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitLoop(this);}
}
public class Conditional : Stmt {
public readonly Expr condition;
public readonly Stmt thenBranch;
public readonly Stmt? elseBranch;
public Conditional (Expr condition, Stmt thenBranch, Stmt? elseBranch) {
this.condition = condition;
this.thenBranch = thenBranch;
this.elseBranch = elseBranch;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitConditional(this);}
}
public class Function : Stmt {
public readonly Token name;
public readonly List<Token> parameters;
public readonly List<Stmt> body;
public Function (Token name, List<Token> parameters, List<Stmt> body) {
this.name = name;
this.parameters = parameters;
this.body = body;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitFunction(this);}
}
public class Returning : Stmt {
public readonly Token keyword;
public readonly Expr? value;
public Returning (Token keyword, Expr? value) {
this.keyword = keyword;
this.value = value;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitReturning(this);}
}
}
}