﻿using System.Collections.Generic;
using alps.net.api.util;
using alps.net.api.StandardPASS;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.SubjectBehaviors;
using alps.net.api.parsing;
using alps.net.api.src;

namespace alps.net.api.ALPS.ALPSModelElements.ALPSSBDComponents
{

    class GuardReceiveState : ReceiveState, IGuardReceiveState
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        private const string className = "GuardReceiveState";

        public override string getClassName()
        {
            return className;
        }

        protected override string getExportTag()
        {
            return OWLTags.abstr;
        }
        public override IParseablePASSProcessModelElement getParsedInstance()
        {
            return new GuardReceiveState();
        }

       protected GuardReceiveState() { }

        /// <summary>
        /// Constructor that creates a new fully specified instance of the guard receive state class
        /// </summary>
        /// <param name="additionalAttribute"></param>
        /// <param name="modelComponentID"></param>
        /// <param name="modelComponentLabel"></param>
        /// <param name="comment"></param>
        /// <param name="subjectBehavior"></param>
        /// <param name="incomingTransition"></param>
        /// <param name="outgoingTransition"></param>
        /// <param name="functionSpecification"></param>
        /// <param name="guardBehavior"></param>
        /// <param name="action"></param>
        /// <param name="receiveFunction"></param>
        public GuardReceiveState(ISubjectBehavior behavior, string labelForID = null, IGuardBehavior guardBehavior = null,
            IReceiveFunction functionSpecification = null,
            ISet<ITransition> incomingTransition = null, ISet<IReceiveTransition> outgoingTransition = null, string comment = null, string additionalLabel = null, IList<IIncompleteTriple> additionalAttribute = null)
            : base(behavior, labelForID, guardBehavior, functionSpecification, incomingTransition, outgoingTransition, comment, additionalLabel, additionalAttribute)
        { }

    }
}
