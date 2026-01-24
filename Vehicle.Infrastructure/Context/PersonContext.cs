using System.Threading;

namespace Vehicle.Infrastructure.Context
{
    public class PersonContext
    {
        private static readonly AsyncLocal<int?> _personId = new AsyncLocal<int?>();
        private static readonly AsyncLocal<int?> _shardId = new AsyncLocal<int?>();

        public int? PersonId
        {
            get => _personId.Value;
            set => _personId.Value = value;
        }

        public int? ShardId
        {
            get => _shardId.Value;
            set => _shardId.Value = value;
        }
    }
}
