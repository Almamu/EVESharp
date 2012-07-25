namespace Destiny
{
    public enum BallMode : byte
    {
        /// <summary>
        /// makes ball attempt to reach the goto point
        /// </summary>
        Goto,

        /// <summary>
        /// makes ball follow another ball
        /// </summary>
        Follow,

        /// <summary>
        /// brings ball to a stop
        /// </summary>
        Stop,

        /// <summary>
        /// moves ball very fast to another location
        /// </summary>
        Warp,

        /// <summary>
        /// orbits another ball at some given range
        /// </summary>
        Orbit,

        /// <summary>
        /// missile tracking a target
        /// </summary>
        Missile,

        /// <summary>
        /// expanding gravity wall
        /// </summary>
        Mushroom,

        /// <summary>
        /// swarm like behavior
        /// </summary>
        Boid,

        /// <summary>
        /// free ball that will become fixed after a while
        /// </summary>
        Troll,

        /// <summary>
        /// ball flagged as mini ball
        /// </summary>
        MiniBall,

        /// <summary>
        /// force field ball
        /// </summary>
        Field,

        /// <summary>
        /// a ball that will never move
        /// </summary>
        Rigid,

        /// <summary>
        /// a ball part of a formation
        /// </summary>
        Formation
    }
}