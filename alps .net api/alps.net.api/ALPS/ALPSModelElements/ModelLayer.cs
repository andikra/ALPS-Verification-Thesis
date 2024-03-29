﻿using alps.net.api.ALPS.ALPSModelElements.ALPSSBDComponents;
using alps.net.api.ALPS.ALPSModelElements.ALPSSIDComponents;
using alps.net.api.parsing;
using alps.net.api.src;
using alps.net.api.StandardPASS;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using alps.net.api.util;
using System.Collections.Generic;
using System.Linq;
using static alps.net.api.ALPS.ALPSModelElements.IModelLayer;

namespace alps.net.api.ALPS.ALPSModelElements
{
    /// <summary>
    /// Class that represents a model layer 
    /// </summary>
    public class ModelLayer : ALPSModelElement, IModelLayer
    {
        protected Dictionary<string, IPASSProcessModelElement> elements = new Dictionary<string, IPASSProcessModelElement>();
        protected readonly IDictionary<string, IModelLayer> implementedInterfaces = new Dictionary<string, IModelLayer>();
        protected int priorityNumber;
        protected IPASSProcessModel model;
        protected LayerType layerType = LayerType.STANDARD;
        protected bool isAbstractType = false;
        protected IModelLayer extendedLayer;

        private const string ABSTRACT_NAME = "AbstractLayer";

        public override IParseablePASSProcessModelElement getParsedInstance()
        {
            return new ModelLayer();
        }

       protected ModelLayer() { }
        /// <summary>
        /// Name of the class
        /// </summary>
        private const string className = "ModelLayer";
        protected string exportClassname = className;

        public override string getClassName()
        {
            return exportClassname;
        }
        protected override string getExportTag()
        {
            return OWLTags.abstr;
        }


        public ModelLayer(IPASSProcessModel model, string labelForID = null, string comment = null, string additionalLabel = null, IList<IIncompleteTriple> additionalAttribute = null)
            : base(labelForID, comment, additionalLabel, additionalAttribute) { setContainedBy(model); }

        public override string getBaseURI()
        {
            if (model != null && model is IParseablePASSProcessModelElement element)
                return element.getBaseURI();
            return base.getBaseURI();
        }


        /// <summary>
        /// Returns a dictionary of all elements saved in the model layer
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IPASSProcessModelElement> getElements()
        {
            return new Dictionary<string, IPASSProcessModelElement>(elements);
        }

        public IPASSProcessModelElement getElement(string id)
        {
            if (elements.ContainsKey(id))
            {
                return elements[id];
            }
            return null;
        }

        protected void checkLayerTypes()
        {
            if (elements.OfType<IALPSModelElement>().ToList().Count == 0)
            {
                setIsAbstract(false);
            }
            else
            {
                setIsAbstract(true);
            }
            foreach (IPASSProcessModelElement element in getElements().Values)
            {
                if (element is IGuardExtension)
                {
                    setLayerType(LayerType.GUARD);
                    return;
                }
                else if (element is IMacroExtension)
                {
                    setLayerType(LayerType.MACRO);
                    return;
                }
                else if (element is ISubjectExtension)
                {
                    setLayerType(LayerType.EXTENSION);
                    return;
                }

            }
            setLayerType(LayerType.STANDARD);
        }


        public virtual void setLayerType(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.GUARD:
                    removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    exportClassname = "Guard" + className;
                    this.layerType = layerType;
                    addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    break;
                case LayerType.EXTENSION:
                    removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    this.layerType = layerType;
                    exportClassname = "Extension" + className;
                    addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    break;
                case LayerType.MACRO:
                    removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    this.layerType = layerType;
                    exportClassname = "Macro" + className;
                    addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    break;
                case LayerType.BASE:
                    removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    this.layerType = layerType;
                    exportClassname = "Base" + className;
                    addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    break;
                case LayerType.STANDARD:
                    removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    this.layerType = layerType;
                    exportClassname = className;
                    addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + getClassName()));
                    break;
                default:

                    break;

            }
        }

        public virtual LayerType getLayerType()
        {
            return layerType;
        }

        public void setIsAbstract(bool isAbstract)
        {
            this.isAbstractType = isAbstract;
            if (isAbstract)
            {
                addTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + ABSTRACT_NAME));
            }
            else
            {
                removeTriple(new IncompleteTriple(OWLTags.rdfType, getExportTag() + ABSTRACT_NAME));
            }
        }

        public bool isAbstract()
        {
            return isAbstractType;
        }

        public bool removeContainedElement(string modelComponentID, int removeCascadeDepth = 0)
        {
            if (modelComponentID is null) return false;
            if (elements.TryGetValue(modelComponentID, out IPASSProcessModelElement element))
            {
                elements.Remove(modelComponentID);
                if (getContainedBy(out IPASSProcessModel model))
                    model.removeElement(modelComponentID);
                element.unregister(this, removeCascadeDepth);
                foreach (IPASSProcessModelElement otherComponent in getElements().Values)
                {
                    otherComponent.updateRemoved(element, this, removeCascadeDepth);
                }
                removeTriple(new IncompleteTriple(OWLTags.stdContains, element.getUriModelComponentID()));
                checkLayerTypes();

            }
            return false;


        }

        /// <summary>
        /// Adds an IPASSProcessModelElement to the layer.
        /// Only elements of type <see cref="IFullySpecifiedSubject"/>, <see cref="IInterfaceSubject"/>, <see cref="IMultiSubject"/>, <see cref="ISingleSubject"/>,
        /// <see cref="IMessageExchange"/>, <see cref="IInputPoolConstraint"/>, <see cref="IMessageSenderTypeConstraint"/>, <see cref="IMessageTypeConstraint"/>,
        /// <see cref="ISenderTypeConstraint"/>, <see cref="IInputPoolConstraintHandlingStrategy"/>, <see cref="IMessageExchangeList"/>, <see cref="IMessageSpecification"/> are allowed to be added.
        /// </summary>
        /// <param name="element">The element that will be added</param>
        public void addElement(IPASSProcessModelElement element)
        {
            if (element is null) { return; }
            if (element is IInteractionDescribingComponent || element is ISubjectBehavior)
                if (elements.TryAdd(element.getModelComponentID(), element))
                {
                    if (element is ISubjectExtension subjExt)
                    {
                        if (!(element is IMacroExtension) && getLayerType() == LayerType.MACRO
                            || !(element is IGuardExtension) && getLayerType() == LayerType.GUARD
                            || (element is IMacroExtension || element is IGuardExtension) && getLayerType() == LayerType.EXTENSION
                            || (element is IGuardReceiveState) && getLayerType() != LayerType.GUARD)
                        {
                            elements.Remove(element.getModelComponentID());
                            return;
                        }
                        foreach (ISubjectExtension ext in getElements().Values.OfType<ISubjectExtension>())
                        {
                            if (ext.getExtendedSubject() != null && subjExt.getExtendedSubject() != null && ext.getExtendedSubject().Equals(subjExt.getExtendedSubject()))
                            {
                                elements.Remove(element.getModelComponentID());
                                return;
                            }
                        }
                    }
                    publishElementAdded(element);
                    element.register(this);
                    
                    checkLayerTypes();
                    if (element is IContainableElement<IModelLayer> containable)
                        containable.setContainedBy(this);
                    addTriple(new IncompleteTriple(OWLTags.stdContains, element.getUriModelComponentID()));
                }
        }


        /// <summary>
        /// Returns a fully specified subject depending on its position
        /// </summary>
        /// <param name="numberOfElement">the position in the list of subjects</param>
        /// <returns></returns>
        public IFullySpecifiedSubject getFullySpecifiedSubject(int numberOfElement)
        {
            return elements.Values.OfType<IFullySpecifiedSubject>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns></returns>
        public IInterfaceSubject getInterfaceSubject(int numberOfElement)
        {
            return elements.Values.OfType<IInterfaceSubject>().ElementAt(numberOfElement);
        }



        /// <summary>
        /// Returns a multi subject depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns></returns>
        public IMultiSubject getMultiSubject(int numberOfElement)
        {
            return elements.Values.OfType<IMultiSubject>().ElementAt(numberOfElement);
        }




        /// <summary>
        /// Returns a single subject depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns></returns>
        public ISingleSubject getSingleSubject(int numberOfElement)
        {
            return elements.Values.OfType<ISingleSubject>().ElementAt(numberOfElement);
        }

        /// <summary>
        /// Returns a message exchange depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns></returns>
        public IMessageExchange getMessageExchange(int numberOfElement)
        {
            return elements.Values.OfType<IMessageExchange>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns an input pool constraint depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IInputPoolConstraint getInputPoolConstraint(int numberOfElement)
        {
            return elements.Values.OfType<IInputPoolConstraint>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns a message sender type constraint depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IMessageSenderTypeConstraint getMessageSenderTypeConstraint(int numberOfElement)
        {
            return elements.Values.OfType<IMessageSenderTypeConstraint>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns a sender type constraint depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public ISenderTypeConstraint getSenderTypeConstraint(int numberOfElement)
        {
            return elements.Values.OfType<ISenderTypeConstraint>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns a message type constraint depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IMessageTypeConstraint getMessageTypeConstraint(int numberOfElement)
        {
            return elements.Values.OfType<IMessageTypeConstraint>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns a input pool constraint handling strategy depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IInputPoolConstraintHandlingStrategy getInputPoolConstraintHandlingStrategy(int numberOfElement)
        {
            return elements.Values.OfType<IInputPoolConstraintHandlingStrategy>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns the message exchange list depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IMessageExchangeList getMessageExchangeList(int numberOfElement)
        {
            return elements.Values.OfType<IMessageExchangeList>().ElementAt(numberOfElement);
        }


        /// <summary>
        /// Returns a message specification depending on its position
        /// </summary>
        /// <param name="numberOfElement"></param>
        /// <returns>The object</returns>
        public IMessageSpecification getMessageSpecification(int numberOfElement)
        {
            return elements.Values.OfType<IMessageSpecification>().ElementAt(numberOfElement);
        }

        protected override void successfullyParsedElement(IParseablePASSProcessModelElement parsedElement)
        {
            if (parsedElement is IContainableElement<IModelLayer> containable)
                containable.setContainedBy(this);
        }

        protected override bool parseAttribute(string predicate, string objectContent, string lang, string dataType, IParseablePASSProcessModelElement element)
        {
            if (predicate.Contains(OWLTags.type))
            {
                if (objectContent.Contains("MacroLayer"))
                {
                    setLayerType(LayerType.MACRO);
                    return true;
                }
                else if (objectContent.Contains("GuardLayer"))
                {
                    setLayerType(LayerType.GUARD);
                    return true;
                }
                else if (objectContent.Contains("ExtensionLayer"))
                {
                    setLayerType(LayerType.EXTENSION);
                    return true;
                }
                else if (objectContent.Contains("BaseLayer"))
                {
                    setLayerType(LayerType.BASE);
                    return true;
                }
                else if (objectContent.Contains(ABSTRACT_NAME))
                {
                    setIsAbstract(true);
                    return true;
                }
            }
            else if (element != null)
            {
                if (predicate.Contains(OWLTags.contains))
                {
                    addElement(element);
                    return true;
                }
                    if (predicate.Contains(OWLTags.extends) && element is IModelLayer layer)
                    {
                        setExtendedLayer(layer);
                        return true;
                    }
                }
            else
            {
                if (predicate.Contains(OWLTags.hasPriorityNumber))
                {
                    string prio = objectContent;
                    prio = prio.Split('^')[0];
                    setPriorityNumber(int.Parse(prio));
                    return true;
                }
            }
            return base.parseAttribute(predicate, objectContent, lang, dataType, element);
        }

        public new void updateAdded(IPASSProcessModelElement update, IPASSProcessModelElement caller)
        {
            base.updateAdded(update, caller);
            addElement(update);
            if (getContainedBy(out IPASSProcessModel model))
            {
                model.updateAdded(update, caller);
            }
        }

        public new void updateRemoved(IPASSProcessModelElement update, IPASSProcessModelElement caller, int removeCascadeDepth = 0)
        {
            base.updateRemoved(update, caller);
            removeContainedElement(update.getModelComponentID(), removeCascadeDepth);

        }



        public override ISet<IPASSProcessModelElement> getAllConnectedElements(ConnectedElementsSetSpecification specification)
        {
            ISet<IPASSProcessModelElement> baseElements = base.getAllConnectedElements(specification);
            foreach (IPASSProcessModelElement element in getElements().Values)
                baseElements.Add(element);
            if (getContainedBy() != null)
                baseElements.Add(getContainedBy());
            return baseElements;
        }


        public void setPriorityNumber(int nonNegativInteger)
        {
            removeTriple(new IncompleteTriple(OWLTags.stdHasPriorityNumber, this.priorityNumber.ToString(), IncompleteTriple.LiteralType.DATATYPE, OWLTags.xsdDataTypePositiveInteger));
            if (nonNegativInteger > 0) priorityNumber = nonNegativInteger;
            else priorityNumber = 1;
            addTriple(new IncompleteTriple(OWLTags.stdHasPriorityNumber, priorityNumber.ToString(), IncompleteTriple.LiteralType.DATATYPE, OWLTags.xsdDataTypePositiveInteger));
        }

        public int getPriorityNumber()
        {
            return priorityNumber;
        }


        public bool getContainedBy(out IPASSProcessModel model)
        {
            model = this.model;
            return this.model != null;
        }

        public void setContainedBy(IPASSProcessModel container)
        {
            if (container is null) return;
            if (container.Equals(model)) return;
            this.model = container;
            this.model.addLayer(this);
        }

        public IPASSProcessModel getContainedBy()
        {
            return this.model;
        }

        public override void notifyModelComponentIDChanged(string oldID, string newID)
        {
            if (elements.ContainsKey(oldID))
            {
                IPASSProcessModelElement element = elements[oldID];
                elements.Remove(oldID);
                elements.Add(element.getModelComponentID(), element);
            }
            base.notifyModelComponentIDChanged(oldID, newID);
        }

        public void setImplementedInterfaces(ISet<IModelLayer> implementedInterface, int removeCascadeDepth = 0)
        {
            foreach (IModelLayer implInterface in getImplementedInterfaces().Values)
            {
                removeImplementedInterfaces(implInterface.getModelComponentID(), removeCascadeDepth);
            }
            if (implementedInterface is null) return;
            foreach (IModelLayer implInterface in implementedInterface)
            {
                addImplementedInterface(implInterface);
            }
        }

        public void addImplementedInterface(IModelLayer implementedInterface)
        {
            if (implementedInterface is null) { return; }
            if (implementedInterfaces.TryAdd(implementedInterface.getModelComponentID(), implementedInterface))
            {
                publishElementAdded(implementedInterface);
                implementedInterface.register(this);
                addTriple(new IncompleteTriple(OWLTags.abstrImplements, implementedInterface.getUriModelComponentID()));
            }
        }

        public void removeImplementedInterfaces(string id, int removeCascadeDepth = 0)
        {
            if (id is null) return;
            if (implementedInterfaces.TryGetValue(id, out IModelLayer implInterface))
            {
                implementedInterfaces.Remove(id);
                implInterface.unregister(this, removeCascadeDepth);
                removeTriple(new IncompleteTriple(OWLTags.abstrImplements, implInterface.getUriModelComponentID()));
            }
        }

        public IDictionary<string, IModelLayer> getImplementedInterfaces()
        {
            return new Dictionary<string, IModelLayer>(implementedInterfaces);
        }

        public void setExtendedLayer(IModelLayer extendedLayer, int removeCascadeDepth = 0)
        {
            IModelLayer oldExtendedLayer = this.extendedLayer;
            // Might set it to null
            this.extendedLayer = extendedLayer;

            if (oldExtendedLayer != null)
            {
                if (oldExtendedLayer.Equals(extendedLayer)) return;
                removeTriple(new IncompleteTriple(OWLTags.abstrExtends, oldExtendedLayer.getUriModelComponentID()));
            }

            if (!(extendedLayer is null))
            {
                addTriple(new IncompleteTriple(OWLTags.abstrExtends, extendedLayer.getUriModelComponentID()));
            }
        }
    }
}

