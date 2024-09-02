# Introduksjon til Kubernetes 

# 0 - Før oppstart 
Sørg for å ha følgende installert: 
 - Docker, containerd, eller podman 
 - kubectl
 - Ha et lokalt cluster, f eks via minikube
# Del 1: Hallo verden
## 1.1 - Deploy et enkelt API
Før vi starter, la oss se om vi har noen pods kjørende: 
```
kubectl get pod
``` 

I et nytt terminalvindu, følg med på pods, deployments og services:
```
watch -n 0.5 kubectl get pod,deploy,svc
```
Denne vil kjøre kommandoen hvert 0.5 sekund, så du ser når ting skjer i clusteret.


Så kan vi spinne opp helt enkelt API i én pod: 
```
kubectl run --image ghcr.io/varianter/k8s-101:v1.0.1 myapi
```

Voila! Da kjører apiet fint. Men - åssen kan vi få brukt det? 

## 1.2 - Eksponer APIet 
Først - la oss få litt mer info om poden:
```
kubectl get pod -o wide
```

Merk deg IP-addressen. Dette er IPen _inne i clusteret_, som betyr at andre pods kan nå den på den IPen. Vi derimot, kan ikke nå den. 
Derfor skal vi først pinge den fra inni clusteret: 
```
kubectl run --image nginx tmp #Start en pod vi kan kjøre curl fra 
kubectl exec -it tmp -- curl <IP>/info #Ping
kubectl delete pod tmp #Rydde opp
```

Så må vi få eksponert appen utad også
```
kubectl expose pod myapi --target-port 8080 --type NodePort --name myservice
```
Dette lager en _NodePort_ -service som lar oss nå poden via _Noden den kjører på_. Typisk i produksjon vil du heller bruke en _IngressController_ el. 

For Minikube, bruk følgende for å få en link til å nå serivcen vi laget, og kopier den.
```
# Mac: 
minikube service myservice --url | pbcopy
```

Prøv å åpne `<URL>/swagger`  i nettleseren. 


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
