﻿using alps.net.api.ALPS.ALPSModelElements;
using alps.net.api.ALPS.ALPSModelElements.ALPSSIDComponents;
using alps.net.api.parsing;
using alps.net.api.src;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using alps.net.api.StandardPASS.SubjectBehaviors;
using alps.net.api.util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF;

namespace alps.net.api.StandardPASS
{
    /// <summary>
    /// Class that represents a PASS Process Model
    /// This model contains all elements known in the current context.
    /// </summary>
    public class PASSProcessModel : PASSProcessModelElement, IPASSProcessModel
    {
        /// <summary>
        /// All elements held by the model (sum of all elements held by the layers)
        /// </summary>
        protected IDictionary<string, IPASSProcessModelElement> allModelElements = new Dictionary<string, IPASSProcessModelElement>();

        /// <summary>
        /// A default layer, created when an element is added but no layer is specified.
        /// Might be null
        /// </summary>
        protected IModelLayer baseLayer;

        protected IDictionary<string, ISubject> startSubjects = new Dictionary<string, ISubject>();
        protected string baseURI;
        protected IPASSGraph baseGraph;
        protected bool layered;

        /// <summary>
        /// Name of the class
        /// </summary>
        private const string className = "PASSProcessModel";

        public override string getClassName()
        {
            return className;
        }

        public override IParseablePASSProcessModelElement getParsedInstance()
        {
            return new PASSProcessModel();
        }

        protected PASSProcessModel() { }

        /// <summary>
        /// Constructor that creates a new fully specified instance of the pass process modell class
        /// </summary>
        public PASSProcessModel(string baseURI, string labelForID = null, ISet<IMessageExchange> messageExchanges = null, ISet<ISubject> relationsToModelComponent = null,
            ISet<ISubject> startSubject = null, string comment = null, string additionalLabel = null, IList<IIncompleteTriple> additionalAttribute = null)
            : base(labelForID, comment, additionalLabel, additionalAttribute)
        {
            if (!(baseURI is null || baseURI.Equals("")))
            {
                setBaseURI(baseURI);
            }

            if (relationsToModelComponent != null)
                foreach (ISubject subj in relationsToModelComponent)
                    addElement(subj);
            if (messageExchanges != null)
                foreach (IMessageExchange mExch in messageExchanges)
                    addElement(mExch);
            setStartSubjects(startSubject);
        }


        // ######################## All contained elements methods ########################


        public IDictionary<string, IPASSProcessModelElement> getAllElements()
        {
            return new Dictionary<string, IPASSProcessModelElement>(allModelElements);
        }

        public void setAllElements(ISet<IPASSProcessModelElement> elements, int removeCascadeDepth = 0)
        {
            foreach (IPASSProcessModelElement element in this.getAllElements().Values)
            {
                removeElement(element.getModelComponentID(), removeCascadeDepth);
            }
            if (elements is null) return;
            foreach (IPASSProcessModelElement element in elements)
            {
                addElement(element);
            }
        }



        public void addElement(IPASSProcessModelElement pASSProcessModelElement, string layerID = null)
        {
            if (pASSProcessModelElement is null) return;


            // Check for layer, create default if non matches

            if (justAddElement(pASSProcessModelElement))
            {

                if (pASSProcessModelElement is IModelLayer)
                {
                    if (getModelLayers().Count > 1) setIsLayered(true);

                }
                else
                {
                    if (pASSProcessModelElement is IInteractionDescribingComponent || pASSProcessModelElement is ISubjectBehavior)
                    {
                        if (layerID is null)
                        {
                            layerID = getBaseLayer().getModelComponentID();
                        }
                        justAddElementToLayer(pASSProcessModelElement, layerID);
                    }
                }
                // if added, register and send signal to next higher observers so they can add and register as well
                pASSProcessModelElement.register(this);
                publishElementAdded(pASSProcessModelElement);
                if (exportGraph != null && pASSProcessModelElement is IParseablePASSProcessModelElement parseable) parseable.setExportGraph(ref exportGraph);
                if (pASSProcessModelElement is IInteractionDescribingComponent || pASSProcessModelElement is IModelLayer || pASSProcessModelElement is ISubjectBehavior)
                    addTriple(new IncompleteTriple(OWLTags.stdContains, pASSProcessModelElement.getUriModelComponentID()));
            }
        }

        /// <summary>
        /// this method is used to just add the element to the list, and not trigger any observers etc.
        /// </summary>
        /// <param name="pASSProcessModelElement">the element that should be added</param>
        /// <returns>true if it could be added, false if not</returns>
        protected bool justAddElement(IPASSProcessModelElement pASSProcessModelElement)
        {
            if (pASSProcessModelElement is null) { return false; }
            if (pASSProcessModelElement.Equals(this)) return false;
            if (!allModelElements.ContainsKey(pASSProcessModelElement.getModelComponentID()))
            {
                allModelElements.TryAdd(pASSProcessModelElement.getModelComponentID(), pASSProcessModelElement);
                if (pASSProcessModelElement is IContainableElement<IPASSProcessModel> containable)
                    containable.setContainedBy(this);
                if (pASSProcessModelElement is IParseablePASSProcessModelElement parseable)
                    parseable.setExportGraph(ref baseGraph);

                return true;
            }
            return false;
        }

        public void removeElement(string modelComponentID, int removeCascadeDepth = 0)
        {
            if (modelComponentID is null) { return; }
            if (allModelElements.TryGetValue(modelComponentID, out IPASSProcessModelElement element))
            {
                if (justRemoveElement(element))
                {
                    if (element is IModelLayer)
                    {
                        if (getModelLayers().Count < 2) setIsLayered(false);
                    }
                    removeTriple(new IncompleteTriple(OWLTags.stdContains, element.getUriModelComponentID()));
                    element.unregister(this, removeCascadeDepth);
                    foreach (IPASSProcessModelElement otherComponent in getAllElements().Values)
                    {
                        otherComponent.updateRemoved(element, this, removeCascadeDepth);
                    }
                    if (getBaseLayer() != null)
                        if (getBaseLayer().getElements().Count() == 0)
                        {
                            baseLayer.removeFromEverything();
                        }
                    element.removeFromEverything();
                }
            }
        }


        /// <summary>
        /// this method is used to just remove the element from the list, and not trigger any observers etc.
        /// </summary>
        /// <param name="element">the element being removed</param>
        /// <returns>true if it could be removed, false if not</returns>
        protected bool justRemoveElement(IPASSProcessModelElement element)
        {
            if (element is null) { return false; }
            if (allModelElements.Remove(element.getModelComponentID()))
            {
                foreach (IModelLayer layer in getModelLayers().Values)
                    if (layer.getElements().ContainsKey(element.getModelComponentID()))
                        layer.removeContainedElement(element.getModelComponentID());
                return true;
            }
            return false;
        }



        // ######################## StartSubject methods ########################


        public void addStartSubject(ISubject startSubject)
        {
            if (startSubject is null) { return; }
            if (startSubjects.TryAdd(startSubject.getModelComponentID(), startSubject))
            {
                addElement(startSubject);
                addTriple(new IncompleteTriple(OWLTags.stdHasStartSubject, startSubject.getUriModelComponentID()));
            }
        }

        public void setStartSubjects(ISet<ISubject> startSubjects, int removeCascadeDepth = 0)
        {
            foreach (ISubject startSubj in this.startSubjects.Values)
            {
                removeStartSubject(startSubj.getModelComponentID(), removeCascadeDepth);
            }
            if (startSubjects is null) return;
            foreach (ISubject startSubj in startSubjects)
            {
                addStartSubject(startSubj);
            }
        }

        public void removeStartSubject(string id, int removeCascadeDepth = 0)
        {
            if (id is null) return;
            if (startSubjects.TryGetValue(id, out ISubject subj))
            {
                // Do not remove the element completely, only remove it as start subject
                //removeElement(id, removeCascadeDepth);
                startSubjects.Remove(id);
                removeTriple(new IncompleteTriple(OWLTags.stdHasStartSubject, subj.getUriModelComponentID()));
            }
        }

        public IDictionary<string, ISubject> getStartSubjects()
        {
            return new Dictionary<string, ISubject>(startSubjects);
        }


        // ######################## Contained layer methods ########################


        public void setLayers(ISet<IModelLayer> modelLayers, int removeCascadeDepth = 0)
        {
            foreach (IModelLayer layer in getModelLayers().Values)
            {
                removeLayer(layer.getModelComponentID(), removeCascadeDepth);
            }
            if (modelLayers is null) return;
            foreach (IModelLayer layer in modelLayers)
            {
                addLayer(layer);
            }
        }

        public void removeLayer(string id, int removeCascadeDepth = 0)
        {
            removeElement(id);
        }

        public IDictionary<string, IModelLayer> getModelLayers()
        {
            IDictionary<string, IModelLayer> resultDict = new Dictionary<string, IModelLayer>();
            foreach (IModelLayer layer in getAllElements().Values.OfType<IModelLayer>())
            {
                resultDict.Add(layer.getModelComponentID(), layer);
            }
            return resultDict;
        }

        public void addLayer(IModelLayer layer)
        {
            addElement(layer);
        }




        public IModelLayer getBaseLayer()
        {
            if (baseLayer is null)
            {
                baseLayer = new ModelLayer(this, "defaultBaseLayer");

            }
            return baseLayer;
        }

        public void setBaseLayer(IModelLayer layer)
        {
            addElement(layer);
            this.baseLayer = layer;
        }





        /// <summary>
        /// this method is used to just add the element to the list, and not trigger any observers etc.
        /// </summary>
        /// <param name="pASSProcessModelElement">the element that should be added</param>
        /// <returns>true if it could be added, false if not</returns>
        protected bool justAddElementToLayer(IPASSProcessModelElement pASSProcessModelElement, string layerID)
        {
            if (pASSProcessModelElement is null) { return false; }
            if (layerID is null) { return false; }
            if (getModelLayers().ContainsKey(layerID))
            {
                getAllElements().TryGetValue(layerID, out IPASSProcessModelElement element);
                if (element is IModelLayer layer)
                {
                    layer.addElement(pASSProcessModelElement);
                    return true;
                }
            }
            return false;
        }



        public override string getBaseURI()
        {
            return baseURI;
        }


        public void setBaseURI(string baseURI)
        {
            if (baseURI != null)
            {
                string formattedBaseURI = baseURI.Trim().Replace(" ", "_");
                this.baseURI = formattedBaseURI;
                if (getBaseGraph() is null)
                {
                    this.baseGraph = new PASSGraph(baseURI);
                    setExportGraph(ref baseGraph);
                }
            }
        }

        public void setIsLayered(bool layered)
        {
            this.layered = layered;
        }

        public bool isLayered()
        {
            return layered;
        }




        public override void completeObject(ref IDictionary<string, IParseablePASSProcessModelElement> allElements)
        {
            bool checkLayers = false;

            IList<IStateReference> refList = new List<IStateReference>();
            IList<IState> newStates = new List<IState>();
            foreach (ParsedStateReferenceStub reference in allElements.Values.OfType<ParsedStateReferenceStub>())
            {
                newStates.Add(reference.transformToState(allElements));
                refList.Add(reference);
            }

            foreach (IStateReference reference in refList)
                allElements.Remove(reference.getModelComponentID());

            foreach (IState state in newStates)
                if (state is IParseablePASSProcessModelElement parseabelState)
                    allElements.Add(state.getModelComponentID(), parseabelState);

            // Go through triples, filter all Layers
            foreach (Triple triple in getTriples())
            {
                string predicateContent = NodeHelper.getNodeContent(triple.Predicate);
                if (predicateContent.Contains(OWLTags.contains))
                {
                    string objectContent = NodeHelper.getNodeContent(triple.Object);

                    string possibleID = objectContent;
                    if (possibleID.Split("#").Length > 1)
                        possibleID = possibleID.Split("#")[possibleID.Split("#").Length - 1];

                    if (allElements.ContainsKey(possibleID))
                    {
                        if (allElements[possibleID] is IModelLayer layer && layer is IParseablePASSProcessModelElement parseable)
                        {
                            // Parse the layer
                            string lang = NodeHelper.getLangIfContained(triple.Object);
                            string dataType = NodeHelper.getDataTypeIfContained(triple.Object);
                            parseAttribute(predicateContent, possibleID, lang, dataType, parseable);
                        }
                    }
                }
            }
            if (getModelLayers().Count == 0)
            {
                getBaseLayer();
                checkLayers = true;
            }
            else
            {
                setIsLayered(true);
                foreach (IModelLayer layer in getModelLayers().Values)
                {

                    // Complete the layer first
                    if (layer is IParseablePASSProcessModelElement parseable)
                        parseable.completeObject(ref allElements);

                    // Find non-abstract layer that is marked as baselayer
                    if (!layer.isAbstract() && layer.getLayerType() == IModelLayer.LayerType.BASE)
                    {
                        this.baseLayer = layer;
                    }

                }
                if (baseLayer is null)
                {
                    // if there was no explicit base layer, find a standard layer and make it base layer
                    foreach (IModelLayer layer in getModelLayers().Values)
                    {
                        if (!layer.isAbstract() && layer.getLayerType() == IModelLayer.LayerType.STANDARD)
                        {
                            this.baseLayer = layer;
                            break;
                        }

                    }
                }
            }

            foreach (Triple triple in getTriples())
            {
                parseAttribute(triple, allElements, out IParseablePASSProcessModelElement element);
                // Calling parsing method
                // If attribute contains a reference to a PassProcessModelElement, pass this to the method

            }




            //foreach (IParseablePASSProcessModelElement element in getAllElements().Values)
            foreach (IParseablePASSProcessModelElement element in allElements.Values)
            {
                if (!(element is IPASSProcessModel))
                    element.completeObject(ref allElements);
            }

            if (checkLayers)
            {
                IDictionary<string, IList<string>> doubleBehaviors = new Dictionary<string, IList<string>>();
                if (getAllElements().Values.OfType<ISubjectBehavior>().Count() > 1)
                    foreach (ISubjectBehavior behavior in getAllElements().Values.OfType<ISubjectBehavior>())
                    {
                        if (behavior.getSubject() != null && behavior.getSubject() is IFullySpecifiedSubject subject)
                        {
                            if (doubleBehaviors.ContainsKey(subject.getModelComponentID()))
                                doubleBehaviors[subject.getModelComponentID()].Add(behavior.getModelComponentID());
                            else doubleBehaviors.Add(subject.getModelComponentID(), new List<string> { behavior.getModelComponentID() });
                        }
                    }
                foreach (KeyValuePair<string, IList<string>> pair in doubleBehaviors)
                {
                    if (pair.Value.Count > 1)
                        fixLayers(pair.Key, pair.Value);

                }
                if (getModelLayers().Count > 1)
                {
                    setIsLayered(true);
                }
            }

            foreach (IParseablePASSProcessModelElement element in getAllElements().Values)
            {
                element.setExportGraph(ref baseGraph);
            }
        }


        private void fixLayers(string idOfSubject, IList<string> idsOfBehaviors)
        {
            /*
             * BaseBehavior Verlinkung
             * Prioritätsnummer
             * 

             */
            IFullySpecifiedSubject extendedSubject = (IFullySpecifiedSubject)getAllElements()[idOfSubject];
            ISubjectBehavior baseBehavior = null;
            if (extendedSubject.getSubjectBaseBehavior() != null)
            {
                // If there is an explicit base behavior
                baseBehavior = extendedSubject.getSubjectBaseBehavior();
            }
            else
            {
                // find a "normal" behavior with the lowest priority number
                int lowestPrio = int.MaxValue;
                foreach (string id in idsOfBehaviors)
                {
                    ISubjectBehavior behavior = (ISubjectBehavior)getAllElements()[id];
                    if (!(behavior is IGuardBehavior || behavior is IMacroBehavior))
                    {
                        if (behavior.getPriorityNumber() < lowestPrio)
                            baseBehavior = behavior;
                    }
                }
            }
            getBaseLayer().addElement(baseBehavior);
            foreach (string id in idsOfBehaviors)
            {
                if (!id.Equals(baseBehavior.getModelComponentID()))
                {
                    ISubjectBehavior behavior = (ISubjectBehavior)getAllElements()[id];
                    IModelLayer layer = new ModelLayer(this);
                    ISubjectExtension ext = null;
                    if (behavior is IMacroBehavior)
                    {
                        layer.setLayerType(IModelLayer.LayerType.MACRO);
                        ext = new MacroExtension(layer);

                    }
                    else if (behavior is IGuardBehavior)
                    {
                        layer.setLayerType(IModelLayer.LayerType.GUARD);
                        ext = new GuardExtension(layer);
                    }
                    else
                    {
                        layer.setLayerType(IModelLayer.LayerType.EXTENSION);
                        ext = new SubjectExtension(layer);
                    }

                    ext.setExtendedSubject(extendedSubject);
                    ext.addExtensionBehavior(behavior);
                    extendedSubject.addBehavior(behavior);
                    layer.addElement(ext);
                    addLayer(layer);
                }
            }
            // TODO
        }

        protected override bool parseAttribute(string predicate, string objectContent, string lang, string dataType, IParseablePASSProcessModelElement element)
        {
            if (predicate.Contains(OWLTags.contains))
            {
                if (element is IModelLayer layer)
                {
                    addLayer(layer);
                }
                else addElement(element);

                return true;
            }
            return base.parseAttribute(predicate, objectContent, lang, dataType, element);
        }

        public override ISet<IPASSProcessModelElement> getAllConnectedElements(ConnectedElementsSetSpecification specification)
        {
            ISet<IPASSProcessModelElement> baseElements = base.getAllConnectedElements(specification);
            foreach (IPASSProcessModelElement component in getAllElements().Values)
                baseElements.Add(component);
            return baseElements;
        }

        public new void updateAdded(IPASSProcessModelElement update, IPASSProcessModelElement caller)
        {
            base.updateAdded(update, caller);

            if (justAddElement(update))
            {
                if (update is IInteractionDescribingComponent || update is ISubjectBehavior)
                {
                    string layerToAdd = null;
                    if (!(caller is IModelLayer))
                    {
                        // Search for the layer containing the caller
                        foreach (IModelLayer layer in getModelLayers().Values)
                        {
                            if (layer.getElements().ContainsKey(caller.getModelComponentID()))
                            {
                                layerToAdd = layer.getModelComponentID();
                                break;
                            }
                        }
                        if (layerToAdd is null)
                        {
                            layerToAdd = getBaseLayer().getModelComponentID();
                        }

                        justAddElementToLayer(update, layerToAdd);
                    }
                }
                // if added, register. Do not send a signal to next higher observers, because they should be already registered on the element that caused the update,
                // and will be informed themselves
                update.register(this);
                if (exportGraph != null && update is IParseablePASSProcessModelElement parseable) parseable.setExportGraph(ref exportGraph);
                //update.completeObject(ref allElements);
            }
            /*else
            {
                if (caller is IModelLayer callingLayer && !callingLayer.Equals(baseLayer))
                {
                    if (baseLayer != null && baseLayer.getElements().ContainsKey(update.getModelComponentID()))
                    {
                        baseLayer.removeContainedElement(update.getModelComponentID());
                    }
                }
            }*/
            if (baseLayer != null && baseLayer.getElements().Count() == 0)
            {
                baseLayer.removeFromEverything();
            }

        }





        public new void updateRemoved(IPASSProcessModelElement update, IPASSProcessModelElement caller, int removeCascadeDepth = 0)
        {
            base.updateRemoved(update, caller, removeCascadeDepth);
            removeElement(update.getModelComponentID(), removeCascadeDepth);

        }







        protected override void successfullyParsedElement(IParseablePASSProcessModelElement parsedElement)
        {
            if (parsedElement is IContainableElement<IPASSProcessModel> containable)
                containable.setContainedBy(this);
            parsedElement.setExportGraph(ref baseGraph);
        }

        public IPASSGraph getBaseGraph()
        {
            return exportGraph;
        }

        public override void notifyModelComponentIDChanged(string oldID, string newID)
        {
            if (allModelElements.ContainsKey(oldID))
            {
                IPASSProcessModelElement element = allModelElements[oldID];
                allModelElements.Remove(oldID);
                allModelElements.Add(element.getModelComponentID(), element);
            }
            base.notifyModelComponentIDChanged(oldID, newID);
        }

        public string export(string filepath)
        {
            // Get the graph hold by the model and use the export function given by the library
            string fullPath = (filepath.EndsWith(".owl")) ? filepath : filepath + ".owl";
            getBaseGraph().exportTo(fullPath);
            FileInfo writtenFile = new FileInfo(fullPath);
            if (File.Exists(fullPath)) return writtenFile.FullName;
            return "";
        }
    }
}

