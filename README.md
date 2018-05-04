# RLOpt framework

This repository contains the Visual Basic implementation of RLOpt, a framework for autonomous reinforcement learning that uses Bayesian optimization. This was presented in 2017 as a conference paper in the Latin American Computer Conference (CLEI). The slides of the presentation of the paper can be found [here](https://github.com/jbarsce/RLOptVB/blob/master/CLEI%20Slides.pdf).

To run RLOpt or other optimizers (or individual agents), choose a Test Class to run from the Main module (for example TestBayesianOptimization), and then choose a test method in order to perform simulations. Two output files are produced after each optimizer meta-episode: a .csv file that stores the raw data of the simulation (the diferent hyper-parameter used among with the function results) and a .txt file that shows the details of each meta-episode.
