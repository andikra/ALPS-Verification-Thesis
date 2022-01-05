﻿using alps.net.api.StandardPASS.DataDescribingComponents;
using System.Collections.Generic;

namespace alps.net.api.StandardPASS.InteractionDescribingComponents
{
    /// <summary>
    /// Interface to the FullySpecifiedSubject class
    /// </summary>

    public interface IFullySpecifiedSubject : ISubject
    {
        /// <summary>
        /// Sets a behavior as BaseBehavior for this subject.
        /// If the behavior is not contained in the list of behaviors, it is also added to the list of normal behaviors.
        /// </summary>
        /// <param name="subjectBaseBehavior">The new BaseBehavior</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        void setBaseBehavior(ISubjectBehavior subjectBaseBehavior, int removeCascadeDepth = 0);

        /// <summary>
        /// Returns the current BaseBehavior.
        /// </summary>
        /// <returns>the current BaseBehavior</returns>
        ISubjectBehavior getSubjectBaseBehavior();

        /// <summary>
        /// Adds a behavior to the current subject.
        /// </summary>
        /// <param name="behavior">The behavior</param>
        /// <returns>a bool indicating whether the process of adding was a success</returns>
        bool addBehavior(ISubjectBehavior behavior);

        /// <summary>
        /// Sets a set of behaviors as Behaviors for this subject, overwriting old behaviors.
        /// </summary>
        /// <param name="behaviors">The set of behaviors</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        void setBehaviors(ISet<ISubjectBehavior> behaviors, int removeCascadeDepth = 0);

        /// <summary>
        /// Removes a behavior from the list of behaviors
        /// </summary>
        /// <param name="id">the id of the behavior</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        /// <returns>a bool indicating whether the process of removal was a success</returns>
        bool removeBehavior(string id, int removeCascadeDepth = 0);

        /// <summary>
        /// Get all behaviors mapped with their ids
        /// </summary>
        /// <returns>A dictionary of behaviors</returns>
        IDictionary<string, ISubjectBehavior> getBehaviors();

        /// <summary>
        /// Sets the Data Definition for this subject
        /// </summary>
        /// <param name="subjectDataDefinition">the Data Definition</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        void setDataDefintion(ISubjectDataDefinition subjectDataDefinition, int removeCascadeDepth = 0);

        /// <summary>
        /// Returns the Data Definition for this subject
        /// </summary>
        /// <returns>the Data Definition</returns>
        ISubjectDataDefinition getSubjectDataDefinition();

        /// <summary>
        /// Adds an input pool constraint to the list of input pool constraints
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns>a bool indicating whether the process of adding was a success</returns>
        bool addInputPoolConstraint(IInputPoolConstraint constraint);

        /// <summary>
        /// Overrides the set of input pool constraints
        /// </summary>
        /// <param name="constraints">the new constraints</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        void setInputPoolConstraints(ISet<IInputPoolConstraint> constraints, int removeCascadeDepth = 0);

        /// <summary>
        /// Removes a specified constraint
        /// </summary>
        /// <param name="id">the id of the input pool constraint</param>
        /// <param name="removeCascadeDepth">Parses the depth of a cascading delete for elements that are connected to the currently deleted one</param>
        /// <returns>a bool indicating whether the process of removal was a success</returns>
        bool removeInputPoolConstraint(string id, int removeCascadeDepth = 0);

        /// <summary>
        /// Returns the input pool constraints mapped with their ids
        /// </summary>
        /// <returns>A dictionary of input pool constraints</returns>
        IDictionary<string, IInputPoolConstraint> getInputPoolConstraints();

    }

}
