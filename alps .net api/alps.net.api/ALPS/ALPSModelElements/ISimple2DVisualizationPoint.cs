﻿
namespace alps.net.api.ALPS.ALPSModelElements
{
    /// <summary>
    /// An interface to define points for a simple visual representation of model elements
    /// </summary>
    public interface ISimple2DVisualizationPoint : IALPSModelElement
    {
        /// <summary>
        /// Gets the relative x coordinate of the point
        /// </summary>
        /// <returns>the relative x coordinate</returns>
        double getRelative2D_PosX();
        /// <summary>
        /// Gets the relative y coordinate of the point
        /// </summary>
        /// <returns>the relative y coordinate</returns>
        double getRelative2D_PosY();
        /// <summary>
        /// Sets the relative x coordinate of the point
        /// </summary>
        /// <param name="posx">the relative x coordinate</param>
        void setRelative2D_PosX(double posx);
        /// <summary>
        /// Sets the relative y coordinate of the point
        /// </summary>
        /// <param name="posy">the relative y coordinate</param>
        void setRelative2D_PosY(double posy);
    }
}
