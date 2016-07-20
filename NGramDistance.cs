using System;
using Augury.Base;

namespace Augury.Lucene
{

    /// <summary>
    /// Licensed to the Apache Software Foundation (ASF) under one or more
    /// contributor license agreements.  See the NOTICE file distributed with
    /// this work for additional information regarding copyright ownership.
    /// The ASF licenses this file to You under the Apache License, Version 2.0
    /// (the "License"); you may not use this file except in compliance with
    /// the License.  You may obtain a copy of the License at
    /// 
    ///     http://www.apache.org/licenses/LICENSE-2.0
    /// 
    /// Unless required by applicable law or agreed to in writing, software
    /// distributed under the License is distributed on an "AS IS" BASIS,
    /// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    /// See the License for the specific language governing permissions and
    /// limitations under the License.
    /// </summary>

    /// <summary>
    /// N-Gram version of edit distance based on paper by Grzegorz Kondrak, 
    /// "N-gram similarity and distance". Proceedings of the Twelfth International 
    /// Conference on String Processing and Information Retrieval (SPIRE 2005), pp. 115-126, 
    /// Buenos Aires, Argentina, November 2005. 
    /// http://www.cs.ualberta.ca/~kondrak/papers/spire05.pdf
    /// 
    /// This implementation uses the position-based optimization to compute partial
    /// matches of n-gram sub-strings and adds a null-character prefix of size n-1 
    /// so that the first character is contained in the same number of n-grams as 
    /// a middle character.  Null-character prefix matches are discounted so that 
    /// strings with no matching characters will return a distance of 0.
    /// 
    /// </summary>
    public class NGramDistance : IStringMetric
    {

        internal readonly int N;

        /// <summary>
        /// Creates an N-Gram distance measure using n-grams of the specified size. </summary>
        /// <param name="size"> The size of the n-gram to be used to compute the string distance. </param>
        public NGramDistance(int size)
        {
            N = size;
        }

        /// <summary>
        /// Creates an N-Gram distance measure using n-grams of size 2.
        /// </summary>
        public NGramDistance()
            : this(2)
        {
        }

        public virtual double Similarity(string s1, string s2)
        {
            if (s1 == s2)
            {
                return 1;
            }

            if (s1 == null || s2 == null)
            {
                return 0;
            }

            var sl = s1.Length;
            var tl = s2.Length;

            if (sl == 0 || tl == 0)
            {
                return sl == tl ? 1 : 0;
            }

            var cost = 0.0;
            if (sl < N || tl < N)
            {
                for (int i = 0, ni = Math.Min(sl, tl); i < ni; i++)
                {
                    if (s1[i] == s2[i])
                    {
                        cost++;
                    }
                }

                return cost/Math.Max(sl, tl);
            }

            var sa = new char[sl + N - 1];

            //construct sa with prefix
            for (var i = 0; i < sa.Length; i++)
            {
                if (i < N - 1)
                {
                    sa[i] = (char)0; //add prefix
                }
                else
                {
                    sa[i] = s1[i - N + 1];
                }
            }
            var p = new double[sl + 1];
            var d = new double[sl + 1];
            
            var tj = new char[N]; // jth n-gram of t

            for (var i = 0; i <= sl; i++)
            {
                p[i] = i;
            }

            for (var j = 1; j <= tl; j++)
            {
                //construct t_j n-gram 
                if (j < N)
                {
                    for (var ti = 0; ti < N - j; ti++)
                    {
                        tj[ti] = (char)0; //add prefix
                    }
                    for (var ti = N - j; ti < N; ti++)
                    {
                        tj[ti] = s2[ti - (N - j)];
                    }
                }
                else
                {
                    tj = s2.Substring(j - N, N).ToCharArray();
                }
                d[0] = j;
                for (var i = 1; i <= sl; i++)
                {
                    cost = 0;
                    var tn = N;
                    //compare sa to t_j
                    for (var ni = 0; ni < N; ni++)
                    {
                        if (sa[i - 1 + ni] != tj[ni])
                        {
                            cost++;
                        }
                        else if (sa[i - 1 + ni] == 0) //discount matches on prefix
                        {
                            tn--;
                        }
                    }
                    var ec = cost/tn;
                    // minimum of cell to the left+1, to the top+1, diagonally left and up +cost
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + ec);
                }
                // copy current distance counts to 'previous row' distance counts
                var temp = p; //placeholder to assist in swapping p and d
                p = d;
                d = temp;
            }

            // our last action in the above loop was to switch d and p, so p now
            // actually has the most recent cost counts
            return 1.0 - p[sl]/Math.Max(tl, sl);
        }

        public override int GetHashCode()
        {
            return (397 * GetType().GetHashCode()) ^ N;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (null == obj || GetType() != obj.GetType())
            {
                return false;
            }
           
            return ((NGramDistance)obj).N == N;
        }
    }

}