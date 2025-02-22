﻿using System.Collections.Generic;
using System.Linq;
using Esprima;
using Esprima.Ast;
using Xunit;

namespace Karambolo.AspNetCore.Bundling.EcmaScript.Internal.Helpers
{
    public class VariableScopeBuilderTest
    {
        [Fact]
        public void FindIdentifer_GlobalScope()
        {
            var moduleContent =
@"import defaultImport, * as namespaceImport from './foo.js';
import { import1, x as aliasImport } from './foo.js';

var globalVar1, globalVar2 = 0;
const [globalConst1, globalConst2] = [0, 1];
let {globalLet1, b: [globalLet2, globalLet3] = [globalConst1 + 3, globalConst2 + 3]} = { globalLet1: 2 };

class GlobalClass1 {
    method(methodParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) {
        var methodLocalVar = 0;
        let methodLocalLet = 1;
    }
}

class GlobalClass2 { }

function globalFunc1(funcParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) { 
  var funcLocalVar = 0;
  let funcLocalLet = 1;

  class FuncLocalClass { }
  
  function funcLocalFunc() { }
}

function globalFunc2() { }

function globalFunc3() { }
";

            Module moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseModule();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("defaultImport"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("namespaceImport"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("import1"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("aliasImport"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalVar1"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalVar2"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalConst1"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalConst2"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalLet1"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalLet2"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalLet3"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("GlobalClass1"));
            Assert.Null(globalBlockScope.FindIdentifier("method"));
            Assert.Null(globalBlockScope.FindIdentifier("methodParam"));
            Assert.Null(globalBlockScope.FindIdentifier("methodLocalVar"));
            Assert.Null(globalBlockScope.FindIdentifier("methodLocalLet"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("GlobalClass2"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalFunc1"));
            Assert.Null(globalBlockScope.FindIdentifier("funcParam"));
            Assert.Null(globalBlockScope.FindIdentifier("funcLocalVar"));
            Assert.Null(globalBlockScope.FindIdentifier("funcLocalLet"));
            Assert.Null(globalBlockScope.FindIdentifier("FuncLocalClass"));
            Assert.Null(globalBlockScope.FindIdentifier("funcLocalFunc"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalFunc2"));
            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("globalFunc3"));
            Assert.Null(globalBlockScope.FindIdentifier("arguments"));
        }

        [Fact]
        public void FindIdentifer_ClassMethodScope()
        {
            var moduleContent =
@"import defaultImport, * as namespaceImport from './foo.js';
import { import1, x as aliasImport } from './foo.js';

var globalVar1, globalVar2 = 0;
const [globalConst1, globalConst2] = [0, 1];
let {globalLet1, b: [globalLet2, globalLet3] = [globalConst1 + 3, globalConst2 + 3]} = { globalLet1: 2 };

class GlobalClass1 {
    method(methodParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) {
        var methodLocalVar = 0;
        let methodLocalLet = 1;
    }
}

class GlobalClass2 { }

function globalFunc1(funcParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) { 
  var funcLocalVar = 0;
  let funcLocalLet = 1;

  class FuncLocalClass { }
  
  function funcLocalFunc() { }
}

function globalFunc2() { }

function globalFunc3() { }
";

            Module moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseModule();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            ClassDeclaration classDeclaration = moduleAst.Body
                .OfType<ClassDeclaration>().Single(d => d.Id?.Name == "GlobalClass1");

            VariableScope classScope = scopes[classDeclaration];
            Assert.IsType<VariableScope.Class>(classScope);
            Assert.Same(globalBlockScope, classScope.ParentScope);

            FunctionExpression functionExpression = classDeclaration.Body.Body
                .OfType<MethodDefinition>().Single(d => d.Key is Identifier id && id.Name == "method").Value
                .As<FunctionExpression>();

            VariableScope methodFunctionScope = scopes[functionExpression];
            Assert.IsType<VariableScope.Function>(methodFunctionScope);
            Assert.Same(classScope, methodFunctionScope.ParentScope);

            BlockStatement blockStatement = functionExpression.Body.As<BlockStatement>();
            
            VariableScope methodBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(methodBlockScope);
            Assert.Same(methodFunctionScope, methodBlockScope.ParentScope);

            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("defaultImport"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("namespaceImport"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("import1"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("aliasImport"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalVar1"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("globalVar2"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalConst1"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("globalConst2"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalLet1"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("globalLet2"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalLet3"));
            Assert.Same(classScope, methodBlockScope.FindIdentifier("GlobalClass1"));
            Assert.Null(methodBlockScope.FindIdentifier("method"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("methodParam"));
            Assert.Same(methodBlockScope, methodBlockScope.FindIdentifier("methodLocalVar"));
            Assert.Same(methodBlockScope, methodBlockScope.FindIdentifier("methodLocalLet"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("GlobalClass2"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalFunc1"));
            Assert.Null(methodBlockScope.FindIdentifier("funcParam"));
            Assert.Null(methodBlockScope.FindIdentifier("funcLocalVar"));
            Assert.Null(methodBlockScope.FindIdentifier("funcLocalLet"));
            Assert.Null(methodBlockScope.FindIdentifier("FuncLocalClass"));
            Assert.Null(methodBlockScope.FindIdentifier("funcLocalFunc"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("globalFunc2"));
            Assert.Same(globalBlockScope, methodBlockScope.FindIdentifier("globalFunc3"));
            Assert.Same(methodFunctionScope, methodBlockScope.FindIdentifier("arguments"));
        }

        [Fact]
        public void FindIdentifer_FunctionScope()
        {
            var moduleContent =
@"import defaultImport, * as namespaceImport from './foo.js';
import { import1, x as aliasImport } from './foo.js';

var globalVar1, globalVar2 = 0;
const [globalConst1, globalConst2] = [0, 1];
let {globalLet1, b: [globalLet2, globalLet3] = [globalConst1 + 3, globalConst2 + 3]} = { globalLet1: 2 };

class GlobalClass1 {
    method(methodParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) {
        var methodLocalVar = 0;
        let methodLocalLet = 1;
    }
}

class GlobalClass2 { }

function globalFunc1(funcParam, aliasImport, globalVar2, globalConst2, globalLet2, globalFunc2, GlobalClass2) { 
  var funcLocalVar = 0;
  let funcLocalLet = 1;

  class FuncLocalClass { }
  
  function funcLocalFunc() { }
}

function globalFunc2() { }

function globalFunc3() { }
";

            Module moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseModule();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            FunctionDeclaration functionDeclaration = moduleAst.Body
                .OfType<FunctionDeclaration>().Single(d => d.Id?.Name == "globalFunc1");

            VariableScope functionScope = scopes[functionDeclaration];
            Assert.IsType<VariableScope.Function>(functionScope);
            Assert.Same(globalBlockScope, functionScope.ParentScope);

            BlockStatement blockStatement = functionDeclaration.Body.As<BlockStatement>();

            VariableScope functionBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(functionBlockScope);
            Assert.Same(functionScope, functionBlockScope.ParentScope);

            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("defaultImport"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("namespaceImport"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("import1"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("aliasImport"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("globalVar1"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("globalVar2"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("globalConst1"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("globalConst2"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("globalLet1"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("globalLet2"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("globalLet3"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("GlobalClass1"));
            Assert.Null(functionBlockScope.FindIdentifier("method"));
            Assert.Null(functionBlockScope.FindIdentifier("methodParam"));
            Assert.Null(functionBlockScope.FindIdentifier("methodLocalVar"));
            Assert.Null(functionBlockScope.FindIdentifier("methodLocalLet"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("GlobalClass2"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("globalFunc1"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("funcParam"));
            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("funcLocalVar"));
            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("funcLocalLet"));
            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("FuncLocalClass"));
            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("funcLocalFunc"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("globalFunc2"));
            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("globalFunc3"));
            Assert.Same(functionScope, functionBlockScope.FindIdentifier("arguments"));
        }

        [Fact]
        public void FindIdentifer_DeclaratorStatementScope()
        {
            var moduleContent =
@"(() => {
  for (var i = 0, n = 1; i < n; i++) {
    const x = i;
    console.log(x);
  }

  for (let x in {[0]: 0}) {
    const x = 1;
    console.log(x);
    var y = -x;
  }

  for (var { y: a, b = y + 4 } of [{ y: y + 3 }])
    var y = (function f() { console.log(a); console.log(b); return a + 2; })();
  
  console.log(y);
})()
";

            Module moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseModule();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            ArrowFunctionExpression functionExpression = moduleAst.Body.Single()
                .As<ExpressionStatement>().Expression
                .As<CallExpression>().Callee
                .As<ArrowFunctionExpression>();

            VariableScope wrapperFunctionScope = scopes[functionExpression];
            Assert.IsType<VariableScope.Function>(wrapperFunctionScope);
            Assert.Same(globalBlockScope, wrapperFunctionScope.ParentScope);

            BlockStatement wrapperFunctionBlockStatement = functionExpression.Body.As<BlockStatement>();

            VariableScope wrapperFunctionBlockScope = scopes[wrapperFunctionBlockStatement];
            Assert.IsType<VariableScope.Block>(wrapperFunctionBlockScope);
            Assert.Same(wrapperFunctionScope, wrapperFunctionBlockScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("n"));
            Assert.Null(wrapperFunctionBlockScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("y"));
            Assert.Null(wrapperFunctionBlockScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("b"));

            // for

            ForStatement forStatement = wrapperFunctionBlockStatement.Body.OfType<ForStatement>().Single();

            VariableScope forScope = scopes[forStatement];
            Assert.IsType<VariableScope.DeclaratorStatement>(forScope);
            Assert.Same(wrapperFunctionBlockScope, forScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, forScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, forScope.FindIdentifier("n"));
            Assert.Null(forScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("y"));
            Assert.Null(forScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, forScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, forScope.FindIdentifier("b"));

            BlockStatement blockStatement = forStatement.Body.As<BlockStatement>();

            VariableScope forBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(forBlockScope);
            Assert.Same(forScope, forBlockScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, forBlockScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, forBlockScope.FindIdentifier("n"));
            Assert.Same(forBlockScope, forBlockScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("y"));
            Assert.Null(forScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, forBlockScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, forBlockScope.FindIdentifier("b"));

            // for in

            ForInStatement forInStatement = wrapperFunctionBlockStatement.Body.OfType<ForInStatement>().Single();

            VariableScope forInScope = scopes[forInStatement];
            Assert.IsType<VariableScope.DeclaratorStatement>(forInScope);
            Assert.Same(wrapperFunctionBlockScope, forInScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, forInScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, forInScope.FindIdentifier("n"));
            Assert.Same(forInScope, forInScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, forInScope.FindIdentifier("y"));
            Assert.Null(forInScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, forInScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, forInScope.FindIdentifier("b"));

            blockStatement = forInStatement.Body.As<BlockStatement>();

            VariableScope forInBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(forInBlockScope);
            Assert.Same(forInScope, forInBlockScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, forInBlockScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, forInBlockScope.FindIdentifier("n"));
            Assert.Same(forInBlockScope, forInBlockScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, forInBlockScope.FindIdentifier("y"));
            Assert.Null(forInBlockScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, forInBlockScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, forInBlockScope.FindIdentifier("b"));

            // for of

            ForOfStatement forOfStatement = wrapperFunctionBlockStatement.Body.OfType<ForOfStatement>().Single();

            VariableScope forOfScope = scopes[forOfStatement];
            Assert.IsType<VariableScope.DeclaratorStatement>(forOfScope);
            Assert.Same(wrapperFunctionBlockScope, forOfScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, forOfScope.FindIdentifier("i"));
            Assert.Same(wrapperFunctionBlockScope, forOfScope.FindIdentifier("n"));
            Assert.Null(forOfScope.FindIdentifier("x"));
            Assert.Same(wrapperFunctionBlockScope, forOfScope.FindIdentifier("y"));
            Assert.Null(forOfScope.FindIdentifier("f"));
            Assert.Same(wrapperFunctionBlockScope, forOfScope.FindIdentifier("a"));
            Assert.Same(wrapperFunctionBlockScope, forOfScope.FindIdentifier("b"));

            Assert.False(scopes.ContainsKey(forOfStatement.Body));
        }

        [Fact]
        public void FindIdentifer_CatchClauseScope()
        {
            var moduleContent =
@"(() => {
  let msg = '';
  
  try { throw {} }
  catch {
    console.log(msg, err);
  }
  
  try { throw {} }
  catch ({msg = 'error'}) {
    var err = {msg};
    console.log(msg, err);
  }
})()
";

            Module moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseModule();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            ArrowFunctionExpression functionExpression = moduleAst.Body.Single()
                .As<ExpressionStatement>().Expression
                .As<CallExpression>().Callee
                .As<ArrowFunctionExpression>();

            VariableScope wrapperFunctionScope = scopes[functionExpression];
            Assert.IsType<VariableScope.Function>(wrapperFunctionScope);
            Assert.Same(globalBlockScope, wrapperFunctionScope.ParentScope);

            BlockStatement wrapperFunctionBlockStatement = functionExpression.Body.As<BlockStatement>();

            VariableScope wrapperFunctionBlockScope = scopes[wrapperFunctionBlockStatement];
            Assert.IsType<VariableScope.Block>(wrapperFunctionBlockScope);
            Assert.Same(wrapperFunctionScope, wrapperFunctionBlockScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("msg"));
            Assert.Same(wrapperFunctionBlockScope, wrapperFunctionBlockScope.FindIdentifier("err"));

            // catch clause #1

            CatchClause catchClause = wrapperFunctionBlockStatement.Body.OfType<TryStatement>().ElementAt(0).Handler;

            VariableScope catchClauseScope = scopes[catchClause];
            Assert.IsType<VariableScope.CatchClause>(catchClauseScope);
            Assert.Same(wrapperFunctionBlockScope, catchClauseScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, catchClauseScope.FindIdentifier("msg"));
            Assert.Same(wrapperFunctionBlockScope, catchClauseScope.FindIdentifier("err"));

            VariableScope catchClauseBlockScope = scopes[catchClause.Body];
            Assert.IsType<VariableScope.Block>(catchClauseBlockScope);
            Assert.Same(catchClauseScope, catchClauseBlockScope.ParentScope);

            Assert.Same(wrapperFunctionBlockScope, catchClauseBlockScope.FindIdentifier("msg"));
            Assert.Same(wrapperFunctionBlockScope, catchClauseBlockScope.FindIdentifier("err"));

            // catch clause #2

            catchClause = wrapperFunctionBlockStatement.Body.OfType<TryStatement>().ElementAt(1).Handler;

            catchClauseScope = scopes[catchClause];
            Assert.IsType<VariableScope.CatchClause>(catchClauseScope);
            Assert.Same(wrapperFunctionBlockScope, catchClauseScope.ParentScope);

            Assert.Same(catchClauseScope, catchClauseScope.FindIdentifier("msg"));
            Assert.Same(wrapperFunctionBlockScope, catchClauseScope.FindIdentifier("err"));

            catchClauseBlockScope = scopes[catchClause.Body];
            Assert.IsType<VariableScope.Block>(catchClauseBlockScope);
            Assert.Same(catchClauseScope, catchClauseBlockScope.ParentScope);

            Assert.Same(catchClauseScope, catchClauseBlockScope.FindIdentifier("msg"));
            Assert.Same(wrapperFunctionBlockScope, catchClauseBlockScope.FindIdentifier("err"));
        }

        [Fact]
        public void FindIdentifer_Hoisting()
        {
            var moduleContent =
@"const foo = 0;

function f1(a = foo) {
  {
    var foo = {};
  }
}

function f2(a = foo) {
  {
    function foo() { return this; }
  }
}

function f3(a) {
  'use strict';
  {
    function foo() { return this; }
  }
}

function f4(a = foo) {
  {
    class foo { }
  }
}
";

            Script moduleAst = new JavaScriptParser(moduleContent, ModuleBundler.CreateParserOptions()).ParseScript();

            var scopes = new Dictionary<Node, VariableScope>();

            var scopeBuilder = new VariableScopeBuilder(scopes);
            scopeBuilder.Visit(moduleAst);

            VariableScope globalBlockScope = scopes[moduleAst];
            Assert.IsType<VariableScope.GlobalBlock>(globalBlockScope);
            Assert.IsType<VariableScope.Global>(globalBlockScope.ParentScope);

            Assert.Same(globalBlockScope, globalBlockScope.FindIdentifier("foo"));

            // f1

            FunctionDeclaration functionDeclaration = moduleAst.Body
                .OfType<FunctionDeclaration>().Single(d => d.Id?.Name == "f1");

            VariableScope functionScope = scopes[functionDeclaration];
            Assert.IsType<VariableScope.Function>(functionScope);
            Assert.Same(globalBlockScope, functionScope.ParentScope);

            Assert.Same(globalBlockScope, functionScope.FindIdentifier("foo"));

            BlockStatement blockStatement = functionDeclaration.Body.As<BlockStatement>();

            VariableScope functionBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(functionBlockScope);
            Assert.Same(functionScope, functionBlockScope.ParentScope);

            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("foo"));

            blockStatement = blockStatement.Body.OfType<BlockStatement>().Single();

            VariableScope nestedBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(nestedBlockScope);
            Assert.Same(functionBlockScope, nestedBlockScope.ParentScope);

            Assert.Same(functionBlockScope, nestedBlockScope.FindIdentifier("foo"));

            // f2

            functionDeclaration = moduleAst.Body
                .OfType<FunctionDeclaration>().Single(d => d.Id?.Name == "f2");

            functionScope = scopes[functionDeclaration];
            Assert.IsType<VariableScope.Function>(functionScope);
            Assert.Same(globalBlockScope, functionScope.ParentScope);

            Assert.Same(globalBlockScope, functionScope.FindIdentifier("foo"));

            blockStatement = functionDeclaration.Body.As<BlockStatement>();

            functionBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(functionBlockScope);
            Assert.Same(functionScope, functionBlockScope.ParentScope);

            Assert.Same(functionBlockScope, functionBlockScope.FindIdentifier("foo"));

            blockStatement = blockStatement.Body.OfType<BlockStatement>().Single();

            nestedBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(nestedBlockScope);
            Assert.Same(functionBlockScope, nestedBlockScope.ParentScope);

            Assert.Same(functionBlockScope, nestedBlockScope.FindIdentifier("foo"));

            // f3

            functionDeclaration = moduleAst.Body
                .OfType<FunctionDeclaration>().Single(d => d.Id?.Name == "f3");

            functionScope = scopes[functionDeclaration];
            Assert.IsType<VariableScope.Function>(functionScope);
            Assert.Same(globalBlockScope, functionScope.ParentScope);

            Assert.Same(globalBlockScope, functionScope.FindIdentifier("foo"));

            blockStatement = functionDeclaration.Body.As<BlockStatement>();

            functionBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(functionBlockScope);
            Assert.Same(functionScope, functionBlockScope.ParentScope);

            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("foo"));

            blockStatement = blockStatement.Body.OfType<BlockStatement>().Single();

            nestedBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(nestedBlockScope);
            Assert.Same(functionBlockScope, nestedBlockScope.ParentScope);

            Assert.Same(nestedBlockScope, nestedBlockScope.FindIdentifier("foo"));

            // f4

            functionDeclaration = moduleAst.Body
                .OfType<FunctionDeclaration>().Single(d => d.Id?.Name == "f4");

            functionScope = scopes[functionDeclaration];
            Assert.IsType<VariableScope.Function>(functionScope);
            Assert.Same(globalBlockScope, functionScope.ParentScope);

            Assert.Same(globalBlockScope, functionScope.FindIdentifier("foo"));

            blockStatement = functionDeclaration.Body.As<BlockStatement>();

            functionBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(functionBlockScope);
            Assert.Same(functionScope, functionBlockScope.ParentScope);

            Assert.Same(globalBlockScope, functionBlockScope.FindIdentifier("foo"));

            blockStatement = blockStatement.Body.OfType<BlockStatement>().Single();

            nestedBlockScope = scopes[blockStatement];
            Assert.IsType<VariableScope.Block>(nestedBlockScope);
            Assert.Same(functionBlockScope, nestedBlockScope.ParentScope);

            Assert.Same(nestedBlockScope, nestedBlockScope.FindIdentifier("foo"));
        }
    }
}
