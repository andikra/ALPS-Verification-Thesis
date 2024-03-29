﻿using alps.net.api.parsing;
using alps.net.api.util;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;

namespace alps.net.api.StandardPASS.DataDescribingComponents
{
    /// <summary>
    /// Class that represents a data object list definition
    /// </summary>
    public class DataObjectListDefinition : DataObjectDefinition, IDataObjectListDefiniton
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        private const string className = "DataObjectListDefintion";


        public override string getClassName()
        {
            return className;
        }
        public override IParseablePASSProcessModelElement getParsedInstance()
        {
            return new DataObjectListDefinition();
        }

       protected DataObjectListDefinition() { }
        public DataObjectListDefinition(IPASSProcessModel model, string labelForID = null, IDataTypeDefinition dataTypeDefintion = null, string comment = null, string additionalLabel = null, IList<IIncompleteTriple> additionalAttribute = null)
            : base(model, labelForID, dataTypeDefintion, comment, additionalLabel, additionalAttribute)
        { }
    }
}
