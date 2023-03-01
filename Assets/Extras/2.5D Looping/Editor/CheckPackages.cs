#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;

namespace Kamgam.Looping25D
{
    public class CheckPackages
    {
        static Action<bool> OnComplete;
        static ListRequest Request;
        static string PackageId;

        public static void CheckForPackage(string packageId, Action<bool> onComplete)
        {
            PackageId = packageId;
            Request = Client.List(offlineMode: true);
            OnComplete = onComplete;

            EditorApplication.update += progress;
        }

        static void progress()
        {
            if (Request.IsCompleted)
            {
                EditorApplication.update -= progress;

                if (Request.Status == StatusCode.Success)
                {
                    bool containsPackage = CheckPackages.containsPackage(Request.Result, PackageId);
                    OnComplete?.Invoke(containsPackage);
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    // Debug.Log("Could not check for packages: " + Request.Error.message);
                    OnComplete.Invoke(false);
                }
            }
        }

        static bool containsPackage(PackageCollection packages, string packageId)
        {
            foreach (var package in packages)
            {
                if (string.Compare(package.name, packageId) == 0)
                    return true;

                foreach (var dependencyInfo in package.dependencies)
                    if (string.Compare(dependencyInfo.name, packageId) == 0)
                        return true;
            }

            return false;
        }
    }
}
#endif