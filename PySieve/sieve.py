#
# 25/9/2019
# ~thwmakos~
#
# prime number sieve in Python

import numpy as np
import matplotlib.pyplot as plt

def sieve(n):
    
    if n < 2:
        return []
    
    # flags[i] is True is i is prime, false otherwise
    flags   = [ True ] * (n + 1)

    # 0 and 1 are not primes
    flags[0] = flags[1] = False 

    for (i, flag) in enumerate(flags):
        if flag: # if still flagged as prime
            # leave it and set all multiples to false
            for j in range(2 * i, n + 1, i):
                flags[j] = False
    
    return list([x for x in range(0, n + 1) if flags[x] == True])

def prime_count(n):
    # get primes list up to n
    primes = sieve(n)
    
    # initialize the list
    # this will be the return value
    # keep in mind that pi[i] is the number
    # of primes less or equal to i + 2
    pi = [ 0 ] * (n - 1)
    
    # pi(2) = 1
    pi[0] = 1 
    # last index into the primes array
    last_index = 1 
    
    # for i = 1, 2, ..., n - 1
    for i in range(1, n - 1):
        # use that pi(n + k) = p(n) + num primes between n and n + k
        
        # primes below (or equal to) i + 2 are at least as many as 
        # the primes below of i + 1
        pi[i] = pi[i - 1]
          
        # are there any more?
        # start looking where we left of 
        for j in range(last_index, len(primes)):
            if primes[j] <= i + 2:
                pi[i]      += 1
                last_index += 1
            else:
                break
            
    return pi   

# 10 ** 5 kills the kernel of the notebook
x_max = 10 ** 5

x = np.arange(2, x_max + 1)
pi = np.asarray(prime_count(x_max))
logx = np.log(x)
# first function
f1 = x / (logx - 1.0)
# second function
f2 = x / (logx - 1.1)
f2[1] = 0.0 # something wrong with f[1], it is -2000 w/e
# third
f3 = x / logx * (1.0 + 1 / logx + 2 / (logx ** 2))

plt.figure()
plt.plot(x, pi)
plt.plot(x, f1)
plt.plot(x, f2)
plt.plot(x, f3)
plt.legend(["$\pi (x)$", "$\\frac{x}{\log x - 1}$", "$\\frac{x}{\log x - 1}$", \
                "$\\frac{x}{\log x} ( 1 + \\frac{1}{\log x} + \\frac{2}{\log^2 x} ) $"])
plt.xlabel("$x$")
plt.show()
