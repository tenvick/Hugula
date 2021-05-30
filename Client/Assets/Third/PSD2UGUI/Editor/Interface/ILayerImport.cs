﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


namespace PSDUINewImporter
{
    // public interface ILayerImport
    // {
    //     void DrawLayer(Layer layer, GameObject parent);
    // }

    public interface IComponentImport
    {
        void DrawLayer(Layer layer, GameObject target, GameObject parent);

    }
}
