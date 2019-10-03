using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace dotnet_vic.core.Model
{
    /// <summary>
    /// Represents the header of dataset file consisting of relation name and feature declarations.
    /// </summary>
    [Serializable]
    public class Header
    {
        /// <summary>
        /// Gets the relation name.
        /// </summary>
        public string RelationName { get; }

        /// <summary>
        /// Gets the declared features.
        /// </summary>
        public ReadOnlyCollection<Feature> Features { get; }

        public Feature ClassFeature { get; }

        public FeatureInformation[] FeaturesInformation { get; set; }

        public Header()
        {
            RelationName = "Unknown";
            Features = new ReadOnlyCollection<Feature>(new List<Feature>());
            ClassFeature = null;
        }

        internal Header(string relationName, IEnumerable<Feature> features)
        {
            RelationName = relationName;
            Features = new ReadOnlyCollection<Feature>(features.ToList());
            FeaturesInformation = Features.Select(feature => feature.FeatureInformation).ToArray();
            ClassFeature = null;
            if (Features.Any(f => f.Name.Equals("Class", StringComparison.InvariantCultureIgnoreCase)))
            {
                ClassFeature = Features.First(f => f.Name.Equals("Class", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public Feature this[string featureName]
        {
            get
            {
                Feature feature = Features.FirstOrDefault(x => x.Name == featureName);
                if (feature == null)
                    throw new InvalidOperationException(string.Format("Feature '{0}' does not exists", featureName));
                return feature;
            }
        }

        public Feature this[int featureIdx]
        {
            get
            {
                return Features[featureIdx];
            }
        }

        /// <summary>
        /// Determines whether this object is equal to another object (an <see cref="Header"/> with the same relation name and features).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Header other))
                return false;

            return other.RelationName == RelationName && other.Features.SequenceEqual(Features);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = RelationName.GetHashCode();

            foreach (Feature feature in Features)
                hashCode = unchecked(hashCode * 31 + feature.GetHashCode());

            return hashCode;
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The string representation of the current object.</returns>
        public override string ToString()
        {
            return $"@relation {Feature.QuoteAndEscape(RelationName)} ({Features.Count} feature{(Features.Count == 1 ? "" : "s")})";
        }
    }
}
