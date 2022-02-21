﻿using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace ModularEncountersSystems.Entities {
    public static class PlanetManager {

        public static List<PlanetEntity> Planets = new List<PlanetEntity>();

        public static PlanetEntity GetNearestPlanet(Vector3D coords) {

            PlanetEntity planet = null;
            bool inGravity = false;
            double distance = -1;

            foreach (var planetEnt in Planets) {

                if (planetEnt?.Planet == null)
                    continue;

                if (planetEnt.IsPositionInGravity(coords)) {

                    planet = planetEnt;
                    inGravity = true;

                } else if (inGravity) {

                    continue;
                
                }

                var thisDist = Vector3D.Distance(planetEnt.Center(), coords);

                if (distance == -1 || thisDist < distance) {

                    planet = planetEnt;
                    distance = thisDist;

                }
            
            }

            return planet;

        }

        public static PlanetEntity GetPlanetWithName(string generatorName) {

            foreach (var planet in Planets) {

                if (!string.IsNullOrWhiteSpace(planet?.Planet?.Generator?.Id.SubtypeName) && planet.Planet.Generator.Id.SubtypeName == generatorName)
                    return planet;
            
            }

            return null;
        
        }

        public static PlanetEntity GetPlanetWithId(long entityId) {

            foreach (var planet in Planets) {

                if (planet?.Planet != null && planet.Planet.EntityId == entityId)
                    return planet;

            }

            return null;

        }

        public static bool InGravity(Vector3D coords) {

            var planet = GetNearestPlanet(coords);

            if (planet != null)
                return planet.IsPositionInGravity(coords);

            return false;

        }

        public static Vector3 GetTotalNaturalGravity(Vector3D coords) {

            Vector3 gravity = Vector3.Zero;

            foreach (var planet in Planets) {

                if (planet == null || planet.Planet.Closed || planet.IsPositionInGravity(coords))
                    continue;

                gravity += planet.Gravity.GetWorldGravity(coords);
            
            }

            return gravity;
        
        }

    }

}
