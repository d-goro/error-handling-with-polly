## Error handling using Polly library
Demo that shows different techniques to handle errors with Polly

Polly is an open source framework for that "allows developers to express transient exception and fault handling policies such as Retry, Retry Forever, Wait and Retry, or Circuit Breaker in a fluent manner". Polly allows us to easily describe fault handling logic by creating a policy to represent the behaviour and then apply the policy to a delegate.

Using Polly in general is really straightforward. We can express retry policies of three types: retry a number of times, retry forever and wait and retry. But most interesting is CircuitBreaker policy.


**Circuit-breakers in brief**

Circuit-breakers make sense when calling a somewhat unreliable API. They use a fail-fast approach when a method has failed several times in a row. The circuit-breaker tracks the number of times an API call has failed. Once it crosses a threshold number of failures in a row, it doesn't even try to call the API for subsequent requests. Instead, it fails immediately, as though the API had failed.

After some timeout, the circuit-breaker will let one method call through to "test" the API and see if it succeeds. If it fails, it goes back to just failing immediately. If it succeeds then the circuit is closed again, and it will go back to calling the API for every request.
