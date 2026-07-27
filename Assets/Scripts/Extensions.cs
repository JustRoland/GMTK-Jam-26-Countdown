using System;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static int IndexOfMin(this IList<float> self)
    {
        if (self == null) {
            throw new ArgumentNullException(nameof(self));
        }

        if (self.Count == 0) {
            throw new ArgumentException("List is empty.", nameof(self));
        }

        float min = self[0];
        int minIndex = 0;

        for (int i = 1; i < self.Count; ++i) {
            if (self[i] < min) {
                min = self[i];
                minIndex = i;
            }
        }

        return minIndex;
    }
}
