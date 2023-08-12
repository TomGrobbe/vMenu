using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public class EntitySpawner : BaseScript
    {
        public static bool Active { get; private set; } = false;
        public static Entity CurrentEntity { get; private set; } = null;
        private int scaleform = 0;
        private readonly float rotateSpeed = 20f;

        private const float RayDistance = 25f;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EntitySpawner()
        {
#if DEBUG
            RegisterCommand("testEntity", new Action<int, List<object>>((source, args) =>
            {
                var prop = (string)args[0];
                SpawnEntity(prop, Game.PlayerPed.Position);
            }), false);

            RegisterCommand("endTest", new Action(() =>
            {
                FinishPlacement();
            }), false);
#endif
        }

        #region PublicMethods

        /// <summary>
        /// Method for spawning entity with EntitySpawner. After entity is spawned you will be able to change
        /// position of entity with your mouse.
        /// </summary>
        /// <param name="model">model of entity as string</param>
        /// <param name="coords">initial coords for the entity</param>
        /// <returns>true spawn was succesful</returns>
        public static void SpawnEntity(string model, Vector3 coords)
        {
            SpawnEntity((uint)GetHashKey(model), coords);
        }

        /// <summary>
        /// Method for spawning entity with EntitySpawner. After entity is spawned you will be able to change
        /// position of entity with your mouse.
        /// </summary>
        /// <param name="model">model of entity as hash</param>
        /// <param name="coords">initial coords for the entity</param>
        /// <returns>true spawn was succesful</returns>
        public static async void SpawnEntity(uint model, Vector3 coords)
        {
            if (!IsModelValid(model))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }

            if (CurrentEntity != null)
            {
                Notify.Error("One entity is currently being processed.");
                return;
            }

            int handle;
            RequestModel(model);
            while (!HasModelLoaded(model))
            {
                await Delay(1);
            }
            if (IsModelAPed(model))
            {
                handle = CreatePed(4, model, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading, true, true);
            }
            else if (IsModelAVehicle(model))
            {
                handle = await CommonFunctions.SpawnVehicle(model, false, false, skipLoad: false, vehicleInfo: new CommonFunctions.VehicleInfo(), saveName: null, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading);
            }
            else
            {
                handle = CreateObject((int)model, coords.X, coords.Y, coords.Z, true, true, true);
            }

            CurrentEntity = Entity.FromHandle(handle);

            if (!CurrentEntity.Exists())
            {
                Notify.Error("Failed to create entity");
                return;
            }

            SetEntityAsMissionEntity(handle, true, true); // Set As mission to prevent despawning

            Active = true;
        }

        /// <summary>
        /// Method used to confirm location of prop and finish placement
        /// </summary>
        public static async void FinishPlacement(bool duplicate = false)
        {
            if (duplicate)
            {
                var hash = CurrentEntity.Model.Hash;
                var position = CurrentEntity.Position;
                CurrentEntity = null;
                await Delay(1); // Mandatory
                SpawnEntity((uint)hash, position);
            }
            else
            {
                Active = false;
                CurrentEntity = null;
            }
        }

        #endregion

        #region InternalMethods

        /// <summary>
        /// Used internally for drawing of help text
        /// </summary>
        private void DrawButtons() //TODO: Right keys
        {
            BeginScaleformMovieMethod(scaleform, "CLEAR_ALL");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(0);
            PushScaleformMovieMethodParameterString("~INPUT_VEH_FLY_ROLL_LR~");
            PushScaleformMovieMethodParameterString("Rotate Object");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
            ScaleformMovieMethodAddParamInt(0);
            EndScaleformMovieMethod();

            DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
        }

        /// <summary>
        /// Used internally for getting direction vector from rotation vector
        /// </summary>
        /// <param name="rotation">Input rotation vector</param>
        /// <returns>Output direction vector</returns>
        private Vector3 RotationToDirection(Vector3 rotation)
        {
            var adj = new Vector3(
                (float)Math.PI / 180f * rotation.X,
                (float)Math.PI / 180f * rotation.Y,
                (float)Math.PI / 180f * rotation.Z
            );

            return new Vector3(
                (float)(-Math.Sin(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)(Math.Cos(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)Math.Sin(adj.X)
            );
        }

        /// <summary>
        /// Used to get coords of reycast from player camera;
        /// </summary>
        /// <returns>destination if no hit was found and coords of hit if there was one</returns>
        private Vector3 GetCoordsPlayerIsLookingAt()
        {
            var camRotation = GetGameplayCamRot(0);
            var camCoords = GetGameplayCamCoord();
            var camDirection = RotationToDirection(camRotation);

            var dest = new Vector3(
                camCoords.X + (camDirection.X * RayDistance),
                camCoords.Y + (camDirection.Y * RayDistance),
                camCoords.Z + (camDirection.Z * RayDistance)
            );

            var res = World.Raycast(camCoords, dest, IntersectOptions.Everything, Game.PlayerPed);

#if DEBUG
            DrawLine(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, dest.X, dest.Y, dest.Z, 255, 0, 0, 255);
#endif

            return res.DitHit ? res.HitPosition : dest;
        }

        #endregion

        /// <summary>
        /// Main tick method for class
        /// </summary>
        [Tick]
        internal async Task MoveHandler()
        {
            if (Active)
            {
                scaleform = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
                while (!HasScaleformMovieLoaded(scaleform))
                {
                    await Delay(0);
                }

                DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 0, 0);
            }
            else
            {
                if (scaleform != 0)
                {
                    SetScaleformMovieAsNoLongerNeeded(ref scaleform); // Unload scaleform if there is no need to draw it
                    scaleform = 0;
                }
            }

            var headingOffset = 0f;
            while (Active)
            {
                if (CurrentEntity == null || !CurrentEntity.Exists())
                {
                    Active = false;
                    CurrentEntity = null;
                    break;
                }
                var handle = CurrentEntity.Handle;

                DrawButtons();

                FreezeEntityPosition(handle, true);
                SetEntityInvincible(handle, true);
                SetEntityCollision(handle, false, false);
                SetEntityAlpha(handle, (int)(255 * 0.4), 0);
                CurrentEntity.Heading = (GetGameplayCamRot(0).Z + headingOffset) % 360f;

                var newPosition = GetCoordsPlayerIsLookingAt();

                CurrentEntity.Position = newPosition;
                if (CurrentEntity.HeightAboveGround < 3.0f)
                {
                    if (CurrentEntity.Model.IsVehicle)
                    {
                        SetVehicleOnGroundProperly(CurrentEntity.Handle);
                    }
                    else
                    {
                        PlaceObjectOnGroundProperly(CurrentEntity.Handle);
                    }
                }

                // Controls
                if (Game.IsControlPressed(0, Control.VehicleFlyRollLeftOnly))
                {
                    headingOffset += rotateSpeed * Game.LastFrameTime;
                }
                else if (Game.IsControlPressed(0, Control.VehicleFlyRollRightOnly))
                {
                    headingOffset -= rotateSpeed * Game.LastFrameTime;
                }

                await Delay(0);

                FreezeEntityPosition(handle, false);
                SetEntityInvincible(handle, false);
                SetEntityCollision(handle, true, true);
                ResetEntityAlpha(handle);
            }

            await Task.FromResult(0);
        }
    }
}