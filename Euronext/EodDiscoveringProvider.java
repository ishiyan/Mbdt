package mbmr.data.provider;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.FileNotFoundException;
import java.io.UnsupportedEncodingException;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.File;
import java.io.ByteArrayInputStream;
import java.io.InputStreamReader;
import java.util.Arrays;
import java.util.Enumeration;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

import mbmr.data.repository.EodQuoteRepository;
import mbmr.data.repository.SecurityInfoRepository;
import mbmr.util.Logger;
import mbmr.util.SharedDocument;
import mbmr.util.ThreadExecutor;


public abstract class EodDiscoveringProvider {

	/**
	 * Reads in all the CSV lines from the {@code reader}. For every CSV line,
	 * retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using the {@link EodQuoteRepository EodQuoteRepository}.
	 *
	 * @param reader the input source reader.
	 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
	 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
	 * @param log the {@linkplain Logger logger}.
	 */
	protected abstract void process(BufferedReader reader, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, Logger log,
		SharedDocument sharedDocument, boolean verbose, int daysBack);

	/**
	 * Checks if the {@code name} matches the valid file name.
	 * @param name the file name to match.
	 */
	public abstract boolean matchFileName(final String name);

	/**
	 * Processes a CSV file in a separate thread. For every line in the CSV file,
	 * retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using
	 * the {@link EodQuoteRepository EodQuoteRepository}.
	 */	
	private class FileRunnable implements Runnable {
		private final File file;
		private final String approvedRepositoryFilePath;
		private final String discoveredRepositoryFilePath;
		private final Logger log;
		private SharedDocument sharedDocument;
		private boolean verbose;
		private final int daysBack;
		
		/**
		 * @param file the input CSV file. The file must be not opened.
		 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
		 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
		 * @param log the {@linkplain Logger logger}.
		 */
		public FileRunnable(File file, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, Logger log, SharedDocument sharedDocument, boolean verbose, int daysBack) {
			this.file = file;
			this.approvedRepositoryFilePath = approvedRepositoryFilePath;
			this.discoveredRepositoryFilePath = discoveredRepositoryFilePath;
			this.log = log;
			this.sharedDocument = sharedDocument;
			this.verbose = verbose;
			this.daysBack = daysBack;
		}

	    public void run() {
			BufferedReader reader = null;
	    	try {
				reader = new BufferedReader(new FileReader(file));
				process(reader, approvedRepositoryFilePath, discoveredRepositoryFilePath, log, sharedDocument, verbose, daysBack);
			} catch (FileNotFoundException e) {
            	StringBuffer buffer = new StringBuffer(512);
            	buffer.append("Failed to find the file \"");
            	buffer.append(file);
            	buffer.append("\": ");
            	buffer.append(e.getMessage());
            	log.error(buffer);
			} catch (Exception e) {
				log.error(e.getMessage());
			} finally {
				if (null != reader) {
					try {
						reader.close();											
					} catch (IOException e) {
						log.error(e.getMessage());						
					}
				}
			}
	    }
	}

	/**
	 * Processes a CSV blob (a file image from an archive) in a separate thread.
	 * For every line in the CSV blob, retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using
	 * the {@link EodQuoteRepository EodQuoteRepository}.
	 */	
	protected class BlobRunnable implements Runnable {
		private final byte[] blob;
		private final String approvedRepositoryFilePath;
		private final String discoveredRepositoryFilePath;
		private final Logger log;
		private SharedDocument sharedDocument;
		private boolean verbose;
		private final int daysBack;
		
		/**
		 * @param blob the input CSV file. The file must be not opened.
		 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
		 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
		 * @param log the {@linkplain Logger logger}.
		 */
		public BlobRunnable(byte[] blob, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, Logger log, SharedDocument sharedDocument, boolean verbose, int daysBack) {
			this.blob = blob;
			this.approvedRepositoryFilePath = approvedRepositoryFilePath;
			this.discoveredRepositoryFilePath = discoveredRepositoryFilePath;
			this.log = log;
			this.sharedDocument = sharedDocument;
			this.verbose = verbose;
			this.daysBack = daysBack;
		}

	    public void run() {
			BufferedReader reader = null;
	    	try {
				reader = new BufferedReader(new InputStreamReader(new ByteArrayInputStream(blob), "UTF-8"));
				process(reader, approvedRepositoryFilePath, discoveredRepositoryFilePath, log, sharedDocument, verbose, daysBack);
			} catch (UnsupportedEncodingException e) {
				// Impossible
				log.error(e.getMessage());
			} catch (Exception e) {
				log.error(e.getMessage());
			} finally {
				if (null != reader) {
					try {
						reader.close();											
					} catch (IOException e) {
						log.error(e.getMessage());						
					}
				}
			}
	    }
	}

	/**
	 * Recurses the {@code directory}. Processes every file that matches the file names.
	 *
	 * @param directory the input directory to recurse.
	 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
	 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
	 * @param executor the {@linkplain ThreadExecutor ThreadExecutor}.
	 * @param log the {@linkplain Logger logger}.
	 */
	private void recurseDirectory(File directory, String approvedRepositoryFilePath, String discoveredRepositoryFilePath,
		ThreadExecutor executor, Logger log, SharedDocument sharedDocument, StringBuffer buffer, boolean verbose, int daysBack) {
		if (null == buffer) {
        	buffer = new StringBuffer(512);				
		}
		List<File> list = Arrays.asList(directory.listFiles());
		for (File source : list) {
			if (source.isFile()) {
				if (matchFileName(source.getName())) {
					doImport(source, approvedRepositoryFilePath, discoveredRepositoryFilePath, executor, log, sharedDocument, buffer, verbose, daysBack);
				} else {
                	buffer.setLength(0);
                	buffer.append("file \"");
                	buffer.append(source.getAbsolutePath());
                	buffer.append("\": unknown file name pattern, skipping");
                	log.warning(buffer);
				}
			} else if (source.isDirectory()) {
				recurseDirectory(source, approvedRepositoryFilePath, discoveredRepositoryFilePath, executor, log, sharedDocument, buffer, verbose, daysBack);
			}
		}
	}

	/**
	 * Reads in all the CSV lines from the {@code reader}. For every CSV line,
	 * retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using the {@link EodQuoteRepository EodQuoteRepository}.
	 *
	 * @param source the input file (may be a ZIP file) or a folder. The file must be not opened. The folder will be processed recursively.
	 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
	 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
	 * @param executor the {@linkplain ThreadExecutor ThreadExecutor}.
	 * @param log the {@linkplain Logger logger}.
	 */
	@SuppressWarnings("unchecked")
	public void doImport(File source, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, ThreadExecutor executor, Logger log,
		SharedDocument sharedDocument, StringBuffer buffer, boolean verbose, int daysBack) {
		if (source.isFile()) {
			if (null == buffer) {
            	buffer = new StringBuffer(512);				
			}
			if (matchFileName(source.getName())) {
				final String lowerCaseName = source.getName().toLowerCase();
				if (lowerCaseName.endsWith(".csv")) {
					FileRunnable runnable = new FileRunnable(source, approvedRepositoryFilePath, discoveredRepositoryFilePath, log, sharedDocument, verbose, daysBack); 
					if (null != executor) {
						executor.execute(runnable, source.getPath());
					} else {
						Thread thread = Thread.currentThread();
						String name = thread.getName();
						thread.setName(source.getName());
						runnable.run();
						thread.setName(name);
					}
				} else if (lowerCaseName.endsWith(".zip")) {
			    	ZipEntry entry;
			    	ZipFile stream = null;
			    	Enumeration<ZipEntry> enu;
			    	BufferedInputStream bis = null;
			    	String name;
			        byte[] blob = null;
			        long size;
			        int added, read;
			        try {
			        	stream = new ZipFile(source);
			        	enu = (Enumeration<ZipEntry>) stream.entries();
			            while (enu.hasMoreElements()) {
			            	entry = enu.nextElement();
			                if (entry.isDirectory())
			                    continue;
			                name = entry.getName();
			                if (name.toLowerCase().endsWith(".csv")) {
				                size = entry.getSize();
				                if (-1 == size) { // -1 means unknown size
				                	buffer.setLength(0);
				                	buffer.append("zip file \"");
				                	buffer.append(source.getAbsolutePath());
				                	buffer.append("\": entry \"");
				                	buffer.append(name);
				                	buffer.append("\" has unknown size, skipping");
				                	log.error(buffer);
				                	continue;
				                }
				                if (Integer.MAX_VALUE < size) {
				                	buffer.setLength(0);
				                	buffer.append("zip file \"");
				                	buffer.append(source.getAbsolutePath());
				                	buffer.append("\": entry \"");
				                	buffer.append(name);
				                	buffer.append("\" is too large, ");
				                	buffer.append(size);
				                	buffer.append("bytes, skipping");
				                	log.error(buffer);
				                	continue;			                	
				                }
				                blob = new byte[(int)size];
				                added = 0;
				                bis = new BufferedInputStream(stream.getInputStream(entry));
				                while (size > added) {
				                	read = bis.read(blob, added, (int)size - added);
				                    if (-1 == read)
				                        break;
				                    added += read;
				                }
				                bis.close();
				                bis = null;
				                BlobRunnable runnable = new BlobRunnable(blob, approvedRepositoryFilePath, discoveredRepositoryFilePath, log, sharedDocument, verbose, daysBack); 
								if (null != executor) {
				                	buffer.setLength(0);
				                	//buffer.append(source.getCanonicalPath());
				                	buffer.append(source.getPath());
				                	buffer.append( "::");
				                	buffer.append(name);
									executor.execute(runnable, buffer.toString());
								} else {
									Thread thread = Thread.currentThread();
									String threadName = thread.getName();
									thread.setName(source.getName());
									runnable.run();
									thread.setName(threadName);
								}
			                }
			            }
			        } catch (NullPointerException e) {
			        	log.error(e.getMessage());
			        } catch (FileNotFoundException e) {
			        	log.error(e.getMessage());
			        } catch (IOException e) {
			        	log.error(e.getMessage());
			        } catch (Exception e) {
			        	log.error(e.getMessage());
			        } finally {
			        	try {
			        		if (null != bis)
			        			bis.close();
			        		if (null != stream)
			        			stream.close();
			        	} catch (IOException e) {
				        	log.error(e.getMessage());
			        	}
			        }	
				} else {
                	buffer.setLength(0);
                	buffer.append("file \"");
                	buffer.append(source.getAbsolutePath());
                	buffer.append("\" does not match the file name extensions \".zip\" or \".csv\"");
                	log.error(buffer);
				}
			} else {
            	buffer.setLength(0);
            	buffer.append("file \"");
            	buffer.append(source.getAbsolutePath());
            	buffer.append("\" does not match the file name pattern, skipping");
            	log.error(buffer);
			}
		} else if (source.isDirectory()) {
			recurseDirectory(source, approvedRepositoryFilePath, discoveredRepositoryFilePath, executor, log, sharedDocument, buffer, verbose, daysBack);
		}
	}

	/**
	 * Reads in all the CSV lines from the {@code reader}. For every CSV line,
	 * retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using the {@link EodQuoteRepository EodQuoteRepository}.
	 *
	 * @param source the input file (may be a ZIP file) or a folder. The file must be not opened. The folder will be processed recursively.
	 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
	 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created. May be {@code null}; in this case only approved securities will be processed.
	 * @param threadCount the number of the worker threads.
	 * @param log the {@linkplain Logger logger}.
	 * @param eodMixedImport the instance of {@linkplain EodMixedImport EodMixedImport} to get the discovered repository document from.
	 */
	public void doImport(File source, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, int threadCount,
		Logger log, SharedDocument sharedDiscoveredRepositoryDocument, StringBuffer buffer, ThreadGroup threadGroup, boolean verbose, int daysBack) {
		ThreadExecutor executor = null;
		if (1 < threadCount) {
			executor = new ThreadExecutor(threadGroup, threadCount);
		}
		if (source.isFile()) {
			doImport(source, approvedRepositoryFilePath, discoveredRepositoryFilePath,
				executor, log, sharedDiscoveredRepositoryDocument, buffer, verbose, daysBack);
		} else if (source.isDirectory()) {
			recurseDirectory(source, approvedRepositoryFilePath, discoveredRepositoryFilePath, executor,
				log, sharedDiscoveredRepositoryDocument, buffer, verbose, daysBack);
		}
		if (null != executor) {
			executor.waitDone(); // Wait for all the threads to finish			
		}
	}

	public void doImport(List<String> bucket, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, ThreadExecutor executor, Logger log,
		SharedDocument sharedDocument, StringBuffer buffer, boolean verbose, int daysBack) {
		int size = bucket.size();
	    for (int index = 0; index < size; index++) {
	    	doImport(new File(bucket.get(index)), approvedRepositoryFilePath, discoveredRepositoryFilePath, executor, log, sharedDocument, buffer, verbose, daysBack);
	    }
	}

	public void doImport(List<String> bucket, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, int threadCount,
		Logger log, SharedDocument sharedDiscoveredRepositoryDocument, StringBuffer buffer, ThreadGroup threadGroup, boolean verbose, int daysBack) {
		ThreadExecutor executor = null;
		if (1 < threadCount) {
			executor = new ThreadExecutor(threadGroup, threadCount);
		}
		int size = bucket.size();
	    for (int index = 0; index < size; index++) {
	    	doImport(new File(bucket.get(index)), approvedRepositoryFilePath, discoveredRepositoryFilePath,
	    		executor, log, sharedDiscoveredRepositoryDocument, buffer, verbose, daysBack);
	    }
		if (null != executor) {
			executor.waitDone(); // Wait for all the threads to finish			
		}
	}

}
