﻿using System;
using VDS.RDF;

namespace alps.net.api.parsing
{
    /// <summary>
    /// This is an interface for a graph used by each model to back up data in form of triples.
    /// The graph is used mainly for exporting, but could also be used for remote control of the model.
    /// It is always kept up to date when something inside the model changes.
    /// </summary>
    public interface IPASSGraph
    {

        public string getBaseURI();

        public const string EXAMPLE_BASE_URI_PLACEHOLDER = "baseuri:";

        /// <summary>
        /// Adds a triple to the triple store this graph contains
        /// </summary>
        /// <param name="t">the triple</param>
        void addTriple(Triple t);
        /// <summary>
        /// Removes a triple from the triple store this graph contains
        /// </summary>
        /// <param name="t">the triple</param>
        void removeTriple(Triple t);

        /// <summary>
        /// Creates a new Uri node inside the graph
        /// </summary>
        /// <returns>The new Uri node</returns>
        IUriNode createUriNode();
        /// <summary>
        /// Creates a new Uri node from an Uri
        /// </summary>
        /// <param name="uri">The correctly formatted uri</param>
        /// <returns>The new Uri node</returns>
        IUriNode createUriNode(Uri uri);
        /// <summary>
        /// Creates a new Uri node from a string name
        /// This name should not be an uri/url (start with http: ...)
        /// For this use <see cref="createUriNode(Uri)"/>.
        /// </summary>
        /// <param name="qname">The name</param>
        /// <returns>The new Uri node</returns>
        IUriNode createUriNode(string qname);

        ILiteralNode createLiteralNode(string literal);
        ILiteralNode createLiteralNode(string literal, Uri datadef);
        ILiteralNode createLiteralNode(string literal, string langspec);

        

        /// <summary>
        /// Registers a component to the graph.
        /// When a triple is changed, the affected component will be notified and can react
        /// to the change
        /// </summary>
        /// <param name="element">the element that is registered</param>
        void register(IParseablePASSProcessModelElement element);

        /// <summary>
        /// Deregisteres a component previously registered via <see cref="register(IParseablePASSProcessModelElement)"/>
        /// </summary>
        /// <param name="element">the element that is de-registered</param>
        void unregister(IParseablePASSProcessModelElement element);

        /// <summary>
        /// Should be called when a modelComponentID is changed.
        /// The model component ids are like primary keys in a database, and many triples must be updated as result.
        /// Also, the other components inside the model will be notified about the change when they are registered.
        /// </summary>
        /// <param name="oldID">the old id</param>
        /// <param name="newID">the new id</param>
        void modelComponentIDChanged(string oldID, string newID);

        /// <summary>
        /// Exports the current graph as owl to the specified filename.
        /// </summary>
        /// <param name="filepath"></param>
        void exportTo(string filepath);
    }
}