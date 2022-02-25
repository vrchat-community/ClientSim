using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Tests
{
    public static class ClientSimTestUtils
    {
        public static IEnumerator WaitUntil(Func<bool> predicate, string message = "", float timeOut = 1f)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = $"WaitUntil reached timeout! {timeOut}";
            }
            
            float time = Time.time;

            yield return new WaitUntil(() =>
            {
                if (Time.time - time > timeOut)
                {
                    Assert.Fail(message);
                }

                return predicate();
            });
        }
    }
}