# Introduksjon til Kubernetes 

# 0 - Før oppstart 
Sørg for å ha følgende installert: 
 - Docker, containerd, eller podman 
 - kubectl
# Del 1: Hallo verden

 - deploye en applikasjon `k create deploy myapp --image=img`  eller via yaml 
 - Konfigurere med miljøvariabler 
 - Koble på en service og eksponer appen  
 - Skalere opp 

# Del 2: Oppdater
 - Sette på readiness- og livenessProbes
 - Oppdater til neste versjon. Denne krever tilkobling til DB og failer derfor
	 - Her kan man eventuelt velge å bruke en strategi som canary eller blue/green
 - Rull tilbake. 
 - Gå til neste seksjon -> lage en database

-- Pause? 
# Del 3:  Legg på en database 
## 3.1 - Bruk en secret for å legge database-passord, evt connection-string

## 3.2 - Sett opp persistent volum, claim

## 3.3 - Kjør database i cluster 

## 3.4 - Oppdater app til å bruke database


