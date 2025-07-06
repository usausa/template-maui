namespace Template.MobileApp.Behaviors;

using System;
using System.Linq;

using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Maui;

using Smart.Maui.Interactivity;

public sealed class MapsuiBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(MapsuiController),
        typeof(MapsuiBind),
        null,
        propertyChanged: BindChanged);

    public static MapsuiController? GetController(BindableObject bindable) =>
        (MapsuiController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, MapsuiController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not MapControl view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is MapsuiBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new MapsuiBindBehavior());
        }
    }

    private sealed class MapsuiBindBehavior : BehaviorBase<MapControl>
    {
        private MapsuiController? controller;

        protected override void OnAttachedTo(MapControl bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
#pragma warning disable CA2000
                AssociatedObject.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
#pragma warning restore CA2000

                var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(controller.HomeLongitude, controller.HomeLatitude).ToMPoint();
                if (controller.InitialResolution.HasValue)
                {
                    AssociatedObject.Map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate, AssociatedObject.Map.Navigator.Resolutions[controller.InitialResolution.Value]);
                }
                else
                {
                    AssociatedObject.Map.Navigator.CenterOn(sphericalMercatorCoordinate);
                }

                controller.MoveToRequest += OnMoveToRequest;
                controller.ZoomInRequest += OnZoomInRequest;
                controller.ZoomOutRequest += OnZoomOutRequest;
            }
        }

        protected override void OnDetachingFrom(MapControl bindable)
        {
            if (controller is not null)
            {
                controller.MoveToRequest -= OnMoveToRequest;
                controller.ZoomInRequest -= OnZoomInRequest;
                controller.ZoomOutRequest -= OnZoomOutRequest;
            }

            controller = null;

            base.OnDetachingFrom(bindable);
        }

        private void OnMoveToRequest(object? sender, MoveToEventArgs e)
        {
            var mapControl = AssociatedObject;
            if (mapControl is null)
            {
                return;
            }

            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(e.Longitude, e.Latitude).ToMPoint();

            if (e.Resolution.HasValue)
            {
                mapControl.Map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate, mapControl.Map.Navigator.Resolutions[e.Resolution.Value]);
            }
            else
            {
                mapControl.Map.Navigator.CenterOn(sphericalMercatorCoordinate);
            }
        }

        private void OnZoomInRequest(object? sender, EventArgs e)
        {
            var mapControl = AssociatedObject;
            mapControl?.Map.Navigator.ZoomIn();
        }

        private void OnZoomOutRequest(object? sender, EventArgs e)
        {
            var mapControl = AssociatedObject;
            mapControl?.Map.Navigator.ZoomOut();
        }
    }
}
