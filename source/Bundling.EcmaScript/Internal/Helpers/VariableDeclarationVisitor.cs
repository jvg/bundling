﻿using System;
using Esprima.Ast;

namespace Karambolo.AspNetCore.Bundling.EcmaScript.Internal.Helpers
{
    // zero-allocation visitor for finding variable/parameter identifiers in variable/function parameter declarations (including array/object destructuring patterns)
    internal readonly struct VariableDeclarationVisitor<TState>
    {
        private readonly TState _state;
        private readonly Action<TState, Identifier> _visitVariableIdentifier;
        private readonly Action<TState, Expression> _visitRewritableExpression;

        public VariableDeclarationVisitor(TState state, Action<TState, Identifier> visitVariableIdentifier = null, Action<TState, Expression> visitRewritableExpression = null)
        {
            _state = state;
            _visitVariableIdentifier = visitVariableIdentifier ?? delegate { };
            _visitRewritableExpression = visitRewritableExpression ?? delegate { };
        }

        private void VisitBindingExpression(Expression expression)
        {
            switch (expression)
            {
                case Identifier identifier:
                    _visitVariableIdentifier(_state, identifier);
                    break;
                case ArrayPattern arrayPattern:
                    VisitArrayPatternElements(in arrayPattern.Elements);
                    break;
                case ObjectPattern objectPattern:
                    VisitObjectPatternProperties(in objectPattern.Properties);
                    break;
            }
        }

        private void VisitArrayPatternElements(in NodeList<Expression> elements)
        {
            for (var i = 0; i < elements.Count; i++)
                switch (elements[i])
                {
                    case Identifier identifier:
                        _visitVariableIdentifier(_state, identifier);
                        break;
                    case ArrayPattern arrayPattern:
                        VisitArrayPatternElements(in arrayPattern.Elements);
                        break;
                    case ObjectPattern objectPattern:
                        VisitObjectPatternProperties(in objectPattern.Properties);
                        break;
                    case RestElement restElement:
                        VisitBindingExpression(restElement.Argument);
                        break;
                    case AssignmentPattern assignmentPattern:
                        VisitBindingExpression(assignmentPattern.Left);
                        _visitRewritableExpression(_state, assignmentPattern.Right);
                        break;
                }
        }

        private void VisitObjectPatternProperties(in NodeList<Node> properties)
        {
            for (var i = 0; i < properties.Count; i++)
                switch (properties[i])
                {
                    case Property property:
                        if (property.Computed)
                            _visitRewritableExpression(_state, property.Key);

                        switch (property.Value)
                        {
                            case Identifier identifier:
                                _visitVariableIdentifier(_state, identifier);
                                break;
                            case ArrayPattern arrayPattern:
                                VisitArrayPatternElements(in arrayPattern.Elements);
                                break;
                            case ObjectPattern objectPattern:
                                VisitObjectPatternProperties(in objectPattern.Properties);
                                break;
                            case AssignmentPattern assignmentPattern:
                                VisitBindingExpression(assignmentPattern.Left);
                                _visitRewritableExpression(_state, assignmentPattern.Right);
                                break;
                        }
                        break;
                    case RestElement restElement:
                        VisitBindingExpression(restElement.Argument);
                        break;
                }
        }

        public void VisitParam(CatchClause catchClause)
        {
            if (catchClause.Param != null)
                VisitBindingExpression(catchClause.Param);
        }

        public void VisitParams(IFunction function)
        {
            VisitArrayPatternElements(in function.Params);
        }

        public void VisitId(VariableDeclarator variableDeclarator)
        {
            VisitBindingExpression(variableDeclarator.Id);
        }
    }
}
