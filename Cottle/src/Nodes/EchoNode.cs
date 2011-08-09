﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Cottle.Values;

namespace   Cottle.Nodes
{
    sealed class    EchoNode : INode
    {
        #region Attributes

        private IExpression expression;

        #endregion

        #region Constructors

        public  EchoNode (IExpression expression)
        {
            this.expression = expression;
        }

        #endregion

        #region Methods

        public bool Apply (Scope scope, TextWriter output, out Value result)
        {
            output.Write (this.expression.Evaluate (scope, output).AsString);

            result = VoidValue.Instance;

            return false;
        }

        public void Debug (TextWriter output)
        {
            output.Write ("{echo ");
            output.Write (this.expression);
            output.Write ('}');
        }

        #endregion
    }
}
