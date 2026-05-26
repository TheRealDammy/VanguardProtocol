using System;
using System.Collections.Generic;

namespace VanguardProtocol.AbilitySystem
{
    public class GameplayTagContainer
    {
        private readonly HashSet<GameplayTag> _tags = new HashSet<GameplayTag>();

        public event Action<GameplayTag> OnTagAdded;
        public event Action<GameplayTag> OnTagRemoved;

        // -- Mutation --

        public void AddTag(GameplayTag tag)
        {
            if (!tag.IsValid() || _tags.Contains(tag))
                return;

            _tags.Add(tag);
            OnTagAdded?.Invoke(tag);
        }

        public void RemoveTag(GameplayTag tag)
        {
            if (!tag.IsValid() || !_tags.Contains(tag))
                return;
            _tags.Remove(tag);
            OnTagRemoved?.Invoke(tag);
        }

        public void AddTags(GameplayTagContainer other)
        {
            foreach (var tag in other._tags)
                AddTag(tag);
        }

        public void RemoveTags(GameplayTagContainer other)
        {
            foreach (var tag in other._tags)
                RemoveTag(tag);
        }

        // -- Query --

        //Exact match
        public bool HasTag(GameplayTag tag) => _tags.Contains(tag);

        //Check if container has a tag that starts with the query (including hierachy)
        public bool HasTagMatching(GameplayTag tag)
        {
            foreach (var t in _tags)
            {
                if (t.MatchesTag(tag))
                    return true;
            }
            return false;
        }
       public bool HasAllTags(GameplayTagContainer other)
        {
            foreach (var tag in other._tags)
            {
                if (!_tags.Contains(tag))
                    return false;
            }
            return true;
        }
        public bool HasAnyTags(GameplayTagContainer other)
        {
            foreach (var tag in other._tags)
            {
                if (_tags.Contains(tag))
                    return true;
            }
            return false;
        }
        public bool IsEmpty() => _tags.Count == 0;

        public void Clear()
        {
            // Fire Events for each tag removed
            var tagsToRemove = new List<GameplayTag>(_tags);
            foreach (var tag in tagsToRemove)
                RemoveTag(tag);
        }

        public IEnumerable<GameplayTag> GetTags() => _tags;

        public override string ToString()
        {
            return string.Join(", ", _tags);
        }
    }
}


