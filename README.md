# WPFThreads
This is a sample project to show how to use WPF and Task Parallel Library.

Select a target folder, set the minimum word length, and the program will use multiple threads to look for the top 100 most used words in the text files inside the folder.

The code is optimized for 2Gb+ text files and does not lock the main thread.
