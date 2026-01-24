using System.Threading;

namespace Shared.Application.Context
{
    // TODO: Make Person context immutable
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
