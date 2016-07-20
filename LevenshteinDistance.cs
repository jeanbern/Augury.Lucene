using Augury.Base;
using System;

namespace Augury.Lucene
{

    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// Levenstein edit distance class.
    /// </summary>
    public sealed class LevensteinDistance : IStringMetric
    {
        //*****************************
        // Compute Levenshtein distance: see org.apache.commons.lang.StringUtils#getLevenshteinDistance(String, String)
        //*****************************
        public double Similarity(string s1, string s2)
        {
            if (s1 == s2)
            {
                return 1;
            }

            if (s1 == null || s2 == null)
            {
                return 0;
            }
            /*
               The difference between this impl. and the previous is that, rather
               than creating and retaining a matrix of size s.length()+1 by t.length()+1,
               we maintain two single-dimensional arrays of length s.length()+1.  The first, d,
               is the 'current working' distance array that maintains the newest distance cost
               counts as we iterate through the characters of String s.  Each time we increment
               the index of String t we are comparing, d is copied to p, the second int[].  Doing so
               allows us to retain the previous cost counts as required by the algorithm (taking
               the minimum of the cost count to the left, up one, and diagonally up and to the left
               of the current cost count being calculated).  (Note that the arrays aren't really
               copied anymore, just switched...this is clearly much better than cloning an array
               or doing a System.arraycopy() each time  through the outer loop.)
	
               Effectively, the difference between the two implementations is this one does not
               cause an out of memory condition when calculating the LD over two very large strings.
             */

            var sa = s1.ToCharArray();
            var n = sa.Length;
            var p = new int[n + 1];
            var d = new int[n + 1];

            var m = s2.Length;
            if (n == 0 || m == 0)
            {
                return n == m ? 1 : 0;
            }
            
            for (var i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (var j = 1; j <= m; j++)
            {
                var tj = s2[j - 1]; // jth character of t
                d[0] = j;

                for (var i = 1; i <= n; i++)
                {
                    var cost = sa[i - 1] == tj ? 0 : 1; // cost
                    // minimum of cell to the left+1, to the top+1, diagonally left and up +cost
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                var temp = p; //placeholder to assist in swapping p and d
                p = d;
                d = temp;
            }

            // our last action in the above loop was to switch d and p, so p now
            // actually has the most recent cost counts
            return 1.0 - p[n]/(double) Math.Max(s2.Length, sa.Length);
        }

        public override int GetHashCode()
        {
            return 397 * GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (null == obj)
            {
                return false;
            }

            return GetType() == obj.GetType();
        }
    }
}