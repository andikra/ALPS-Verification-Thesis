﻿using alps.net.api.parsing;
using alps.net.api.util;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;

namespace alps.net.api.StandardPASS.BehaviorDescribingComponents
{
    /// <summary>
    /// Class that represents an DoFunction
    /// </summary>

    public class DoFunction : FunctionSpecification, IDoFunction
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        private const string className = "DoFunction";


        public override string getClassName()
        {
            return className;
        }
        public override IParseablePASSProcessModelElement getParsedInstance()
        {
            return new DoFunction();
        }

       protected DoFunction() { }
        public DoFunction(ISubjectBehavior behavior, string labelForID = null, string toolSpecificDefinition = null,
            string comment = null, string additionalLabel = null, IList<IIncompleteTriple> additionalAttribute = null)
            : base(behavior, labelForID,  toolSpecificDefinition, comment, additionalLabel, additionalAttribute) { }

    }
}