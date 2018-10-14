import pandas as pd
from matplotlib import pyplot as plt

def plot():
	squares = pd.read_csv("Saved_data.csv");	
	plt.plot(squares["Gen"], squares["BestScore"])
	plt.plot(squares["Gen"], squares["AverageScore"])
	
	plt.show()


plot()