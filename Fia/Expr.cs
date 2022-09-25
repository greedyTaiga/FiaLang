namespace Fia {
abstract class Expr {
public abstract R Accept<R>(IVisitor<R> visitor);   public interface IVisitor<R>
{
      public R VisitUnary(Expr.Unary unary);
      public R VisitBinary(Expr.Binary binary);
      public R VisitGrouping(Expr.Grouping grouping);
      public R VisitLiteral(Expr.Literal literal);
      public R VisitVariable(Expr.Variable variable);
      public R VisitAssigment(Expr.Assigment assigment);
      public R VisitLogical(Expr.Logical logical);
      public R VisitCall(Expr.Call call);
      public R VisitGet(Expr.Get get);
      public R VisitSet(Expr.Set set);
      public R VisitThisRef(Expr.ThisRef thisref);
}
public class Unary : Expr {
public readonly Token oper;
public readonly Expr right;
public Unary (Token oper, Expr right) {
this.oper = oper;
this.right = right;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitUnary(this);}
}
public class Binary : Expr {
public readonly Expr left;
public readonly Token oper;
public readonly Expr right;
public Binary (Expr left, Token oper, Expr right) {
this.left = left;
this.oper = oper;
this.right = right;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitBinary(this);}
}
public class Grouping : Expr {
public readonly Expr expr;
public Grouping (Expr expr) {
this.expr = expr;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitGrouping(this);}
}
public class Literal : Expr {
public readonly Object? value;
public Literal (Object? value) {
this.value = value;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitLiteral(this);}
}
public class Variable : Expr {
public readonly Token name;
public Variable (Token name) {
this.name = name;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitVariable(this);}
}
public class Assigment : Expr {
public readonly Token name;
public readonly Expr value;
public Assigment (Token name, Expr value) {
this.name = name;
this.value = value;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitAssigment(this);}
}
public class Logical : Expr {
public readonly Expr left;
public readonly Token oper;
public readonly Expr right;
public Logical (Expr left, Token oper, Expr right) {
this.left = left;
this.oper = oper;
this.right = right;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitLogical(this);}
}
public class Call : Expr {
public readonly Expr callee;
public readonly Token paren;
public readonly List<Expr> arguments;
public Call (Expr callee, Token paren, List<Expr> arguments) {
this.callee = callee;
this.paren = paren;
this.arguments = arguments;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitCall(this);}
}
public class Get : Expr {
public readonly Expr obj;
public readonly Token name;
public Get (Expr obj, Token name) {
this.obj = obj;
this.name = name;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitGet(this);}
}
public class Set : Expr {
public readonly Expr obj;
public readonly Token name;
public readonly Expr value;
public Set (Expr obj, Token name, Expr value) {
this.obj = obj;
this.name = name;
this.value = value;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitSet(this);}
}
public class ThisRef : Expr {
public readonly Token keyword;
public ThisRef (Token keyword) {
this.keyword = keyword;
}
public override R Accept<R>(IVisitor<R> visitor){
return visitor.VisitThisRef(this);}
}
}
}