namespace Vehicle.Application.Models
{
    public class ShardingSettings
    {
        /// <summary>
        /// Total number of shards available in the system
        /// </summary>
        public int TotalShards { get; set; } = 1;

        /// <summary>
        /// Hot shard ID to prioritize for new user assignments.
        /// If set, new users will be assigned to this shard instead of random distribution.
        /// Useful for directing traffic to specific shards for load balancing or maintenance.
        /// </summary>
        public int? HotShard { get; set; }
    }
}
