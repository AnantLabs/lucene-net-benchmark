/**
 * Copyright 2005 The Apache Software Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace Lucene.Net.Benchmark.quality {

	/**
	 * Results of quality benchmark run for a single query or for a set of queries.
	 */
	public class QualityStats {
		  /** Number of points for which precision is computed. */
		  public const int MAX_POINTS = 20;
  
		  private double maxGoodPoints;
		  private double recall;
		  private double[] pAt;
		  private double pReleventSum = 0;
		  private double numPoints = 0;
		  private double numGoodPoints = 0;
		  private double mrr = 0;
		  private long searchTime;
		  private long docNamesExtractTime;

		  /**
		   * A certain rank in which a relevant doc was found.
		   */
		  public class RecallPoint {
			private int rank;
			private double recall;
			public RecallPoint(int rank, double recall) {
			  this.rank = rank;
			  this.recall = recall;
			}
			/** Returns the rank: where on the list of returned docs this relevant doc appeared. */
			public int getRank() {
			  return rank;
			}
			/** Returns the recall: how many relevant docs were returned up to this point, inclusive. */
			public double getRecall() {
			  return recall;
			}
		  }
  
		  private ArrayList recallPoints;
  
		  /**
		   * Construct a QualityStats object with anticipated maximal number of relevant hits. 
		   * @param maxGoodPoints maximal possible relevant hits.
		   */
		  public QualityStats(double maxGoodPoints, long searchTime) {
			this.maxGoodPoints = maxGoodPoints;
			this.searchTime = searchTime;
			this.recallPoints = new ArrayList();
			pAt = new double[MAX_POINTS+1]; // pAt[0] unused. 
		  }

		  /**
		   * Add a (possibly relevant) doc.
		   * @param n rank of the added doc (its ordinal position within the query results).
		   * @param isRelevant true if the added doc is relevant, false otherwise.
		   */
		  public void addResult(int n, bool isRelevant, long docNameExtractTime) {
			if (Math.Abs(numPoints+1 - n) > 1E-6) {
			  throw new ArgumentException("point "+n+" illegal after "+numPoints+" points!");
			}
			if (isRelevant) {
			  numGoodPoints+=1;
			  recallPoints.Add(new RecallPoint(n,numGoodPoints));
			  if (recallPoints.Count==1 && n<=5) { // first point, but only within 5 top scores. 
				mrr =  1.0 / n;
			  }
			}
			numPoints = n;
			double p = numGoodPoints / numPoints;
			if (isRelevant) {
			  pReleventSum += p;
			}
			if (n<pAt.Length) {
			  pAt[n] = p;
			}
			recall = maxGoodPoints<=0 ? p : numGoodPoints/maxGoodPoints;
			docNamesExtractTime += docNameExtractTime;
		  }

		  /**
		   * Return the precision at rank n:
		   * |{relevant hits within first <code>n</code> hits}| / <code>n</code>.
		   * @param n requested precision point, must be at least 1 and at most {@link #MAX_POINTS}. 
		   */
		  public double getPrecisionAt(int n) {
			if (n<1 || n>MAX_POINTS) {
			  throw new ArgumentException("n="+n+" - but it must be in [1,"+MAX_POINTS+"] range!"); 
			}
			if (n>numPoints) {
			  return (numPoints * pAt[(int)numPoints])/n;
			}
			return pAt[n];
		  }

		  /**
		   * Return the average precision at recall points.
		   */
		  public double getAvp() {
			return maxGoodPoints==0 ? 0 : pReleventSum/maxGoodPoints;
		  }
  
		  /**
		   * Return the recall: |{relevant hits found}| / |{relevant hits existing}|.
		   */
		  public double getRecall() {
			return recall;
		  }

		  /**
		   * Log information on this QualityStats object.
		   * @param logger Logger.
		   * @param prefix prefix before each log line.
		   */
		  public void log(String title, int paddLines, TextWriter logger, String prefix) {
			for (int i=0; i<paddLines; i++) {  
			  logger.WriteLine();
			}
			if (title!=null && title.Trim().Length>0) {
			  logger.WriteLine(title);
			}
			prefix = prefix==null ? "" : prefix;
			NumberFormatInfo nf = new NumberFormatInfo();
			nf.NumberDecimalDigits = 3;
			nf.NumberGroupSeparator = ",";
			int M = 19;
			logger.WriteLine(prefix+format("Search Seconds: ",M)+
				fracFormat(((double)searchTime/1000).ToString(nf)));
			logger.WriteLine(prefix+format("DocName Seconds: ",M)+
				fracFormat(((double)docNamesExtractTime/1000).ToString(nf)));
			logger.WriteLine(prefix+format("Num Points: ",M)+
				fracFormat(numPoints.ToString(nf)));
			logger.WriteLine(prefix+format("Num Good Points: ",M)+
				fracFormat(numGoodPoints.ToString(nf)));
			logger.WriteLine(prefix+format("Max Good Points: ",M)+
				fracFormat(maxGoodPoints.ToString(nf)));
			logger.WriteLine(prefix+format("Average Precision: ",M)+
				fracFormat(getAvp().ToString(nf)));
			logger.WriteLine(prefix+format("MRR: ",M)+
				fracFormat(getMRR().ToString(nf)));
			logger.WriteLine(prefix+format("Recall: ",M)+
				fracFormat(getRecall().ToString(nf)));
			for (int i=1; i<(int)numPoints && i<pAt.Length; i++) {
			  logger.WriteLine(prefix+format("Precision At "+i+": ",M)+
				  fracFormat(getPrecisionAt(i).ToString(nf)));
			}
			for (int i=0; i<paddLines; i++) {  
			  logger.WriteLine();
			}
		  }

		  private static String padd = "                                    ";
		  private String format(String s, int minLen) {
			s = (s==null ? "" : s);
			int n = Math.Max(minLen,s.Length);
			return (s+padd).Substring(0,n);
		  }
		  private String fracFormat(String frac) {
			int k = frac.IndexOf('.');
			String s1 = padd+frac.Substring(0,k);
			int n = Math.Max(k,6);
			s1 = s1.Substring(s1.Length-n);
			return s1 + frac.Substring(k);
		  }
  
		  /**
		   * Create a QualityStats object that is the average of the input QualityStats objects. 
		   * @param stats array of input stats to be averaged.
		   * @return an average over the input stats.
		   */
		  public static QualityStats average(QualityStats[] stats) {
			QualityStats avg = new QualityStats(0,0);
			if (stats.Length==0) {
			  // weired, no stats to average!
			  return avg;
			}
			int m = 0; // queries with positive judgements
			// aggregate
			for (int i=0; i<stats.Length; i++) {
			  avg.searchTime += stats[i].searchTime;
			  avg.docNamesExtractTime += stats[i].docNamesExtractTime;
			  if (stats[i].maxGoodPoints>0) {
				m++;
				avg.numGoodPoints += stats[i].numGoodPoints;
				avg.numPoints += stats[i].numPoints;
				avg.pReleventSum += stats[i].getAvp();
				avg.recall += stats[i].recall;
				avg.mrr += stats[i].getMRR();
				avg.maxGoodPoints += stats[i].maxGoodPoints;
				for (int j=1; j<avg.pAt.Length; j++) {
				  avg.pAt[j] += stats[i].getPrecisionAt(j);
				}
			  }
			}
			if( m>0) {
				throw new ApplicationException("Fishy: no \"good\" queries!");
			}
			// take average: times go by all queries, other measures go by "good" queries only.
			avg.searchTime /= stats.Length;
			avg.docNamesExtractTime /= stats.Length;
			avg.numGoodPoints /= m;
			avg.numPoints /= m;
			avg.recall /= m;
			avg.mrr /= m;
			avg.maxGoodPoints /= m;
			for (int j=1; j<avg.pAt.Length; j++) {
			  avg.pAt[j] /= m;
			}
			avg.pReleventSum /= m;                 // this is actually avgp now 
			avg.pReleventSum *= avg.maxGoodPoints; // so that getAvgP() would be correct
    
			return avg;
		  }

		  /**
		   * Returns the time it took to extract doc names for judging the measured query, in milliseconds.
		   */
		  public long getDocNamesExtractTime() {
			return docNamesExtractTime;
		  }

		  /**
		   * Returns the maximal number of good points.
		   * This is the number of relevant docs known by the judge for the measured query.
		   */
		  public double getMaxGoodPoints() {
			return maxGoodPoints;
		  }

		  /**
		   * Returns the number of good points (only relevant points).
		   */
		  public double getNumGoodPoints() {
			return numGoodPoints;
		  }

		  /**
		   * Returns the number of points (both relevant and irrelevant points).
		   */
		  public double getNumPoints() {
			return numPoints;
		  }

		  /**
		   * Returns the recallPoints.
		   */
		  public RecallPoint [] getRecallPoints() {
			return (RecallPoint[]) recallPoints.ToArray();
		  }

		  /**
		   * Returns the Mean reciprocal rank over the queries or RR for a single query.
		   * <p>
		   * Reciprocal rank is defined as <code>1/r</code> where <code>r</code> is the 
		   * rank of the first correct result, or <code>0</code> if there are no correct 
		   * results within the top 5 results. 
		   * <p>
		   * This follows the definition in 
		   * <a href="http://www.cnlp.org/publications/02cnlptrec10.pdf"> 
		   * Question Answering - CNLP at the TREC-10 Question Answering Track</a>.
		   */
		  public double getMRR() {
			return mrr;
		  }

  
		  /**
		   * Returns the search time in milliseconds for the measured query.
		   */
		  public long getSearchTime() {
			return searchTime;
		  }

	}
}
