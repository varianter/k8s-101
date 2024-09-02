# Introduksjon til Kubernetes 

# 0 - Før oppstart 
Sørg for å ha følgende installert: 
 - Docker, containerd, eller podman 
 - kubectl
# Del 1: Hallo verden

## 1.1 - Deploy et enkelt API/nettside
 - deploye en applikasjon `k create deploy myapp --image=img`  eller via yaml 
 - Konfigurere med miljøvariabler 
 - Koble på en service og eksponer appen  
 - Skalere opp 
## 1.2 - Slides om hva vi nettopp gjorde
 Forklar: 
  - Nodes, Control Plane 
  - Deployment, pods 
  - Services 

# Del 2: Oppdater
## 2.1 Gjøre ting 
 - Sette på readiness- og livenessProbes
 - Oppdater til neste versjon. Denne krever tilkobling til DB og failer derfor
	 - Her kan man eventuelt velge å bruke en strategi som canary eller blue/green
 - Rull tilbake. 
 - Gå til neste seksjon -> lage en database
## 2.2 Slides om åssen deployments endres, replicasets osv 

# Pause 
# Del 3:  Legg på en database
// Denne delen kan bli lang - Vi kan vurdere å droppe den. Eller lage noen snarveier kanskje 
## 3.1 - Bruk en secret for å legge database-passord, evt connection-string

## 3.2 - Sett opp persistent volum, claim

## 3.3 - Kjør database i cluster 

## 3.4 - Oppdater app til å bruke database
