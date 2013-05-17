﻿// OsmSharp - OpenStreetMap tools & library.
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace OsmSharp.Collections.Tags
{
    /// <summary>
    /// Abstracts an index containing tags.
    /// </summary>
    public interface ITagsIndex
    {
        /// <summary>
        /// Returns the tags that belong to the given id.
        /// </summary>
        /// <param name="tagsId"></param>
        /// <returns></returns>
        TagsCollection Get(uint tagsId);

        /// <summary>
        /// Adds new tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        uint Add(TagsCollection tags);
    }

    ///// <summary>
    ///// Common extension functions for tags.
    ///// </summary>
    //public static class TagsExtensions
    //{
    //    /// <summary>
    //    /// Converts a list of keyvalue pairs to a dictionary.
    //    /// </summary>
    //    /// <param name="list"></param>
    //    /// <returns></returns>
    //    public static IDictionary<string, string> ConvertToDictionary(this IList<KeyValuePair<string, string>> list)
    //    {
    //        Dictionary<string, string> dictionary = new Dictionary<string, string>();
    //        foreach (KeyValuePair<string, string> pair in list)
    //        {
    //            dictionary[pair.Key] = pair.Value;
    //        }
    //        return dictionary;
    //    }
    //}
}