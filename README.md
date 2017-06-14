# Neleus.LambdaCompare

This project compares two lambda expressions recursively traversing their trees.

It doesn''t use strict expression tree comparison. Instead, it collapses constant expressions and compares their values rather than their trees. It is useful for mocks validation when the lambda has a reference to local variable. In his case the variable is compared by its value.

See origin https://stackoverflow.com/a/24528357/2528649