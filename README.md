# Introduksjon til Kubernetes 

# 0 - Installer pakker
Sørg for å ha følgende installert: 
 - Docker, containerd, eller podman: `brew install docker`
 - kubectl `brew install kubectl`
 - minikube `brew install minikube`

Start clusteret:
```
minikube start --addons ingress
```

Og test tilkoblingen:
```
kubectl get pod
```

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

Voila! Da kjører apiet fint. Men - det kan fortsatt bare nås av andre pods i clusteret. 

## 1.2 - Si Hei
Først - la oss få litt mer info om poden:
```
kubectl get pod -o wide
```

Merk deg IP-addressen. Dette er IPen _inne i clusteret_, som betyr at andre pods kan nå den på den IPen. Vi derimot, kan ikke nå den. 
Derfor skal vi først pinge den fra inni clusteret: 
```
kubectl run --image nginx tmp #Start en pod vi kan kjøre curl fra 
kubectl exec -it tmp -- curl <IP>/info #Ping
```

### 1.2.1 - Test eksperimentelt endepunkt
Vi har også en nytt, _eksperimentelt_ endepunkt, `/experimental`
Kall det via: (Bruk pil opp og endre forrige kommando)
```
kubectl exec -it tmp -- curl <IP>/experimental
```
Og sjekk igjen at `/info` virker:
```
kubectl exec -it tmp -- curl <IP>/info
```

Uuups.. Kanskje vi burde hatt mer robusthet her?
## 1.3 - Rydde opp
Vi kan sikkert bare ta ned APIet, der var vel ingen brukere uansett. Slett unna debug-poden tmp også. 

```
kubectl delete pod myapi tmp
```


## 1.4 - Slides om hva vi nettopp gjorde
 Forklar: 
  - Nodes, Control Plane 
  - Deployment, pods 
  - Services 
  
## 2 - Deployments
## 2.1 - Lag en basic deployment
Før vi går i produksjon, trenger vi åpenbart litt mer robusthet. 
Lag en .yaml-fil med følgende innhold (se `ressurser/yaml-eksempler/deploy-basic.yaml`):
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: simple-api
  name: simple-api-deployment
spec:
  replicas: 2
  selector:
    matchLabels:
      app: simple-api
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: simple-api
    spec:
      containers:
      - image: "ghcr.io/varianter/k8s-101:v1.0.1"
        name: api
        ports:
          - containerPort: 8080
        resources: {}
status: {}
```

Når du har lagret, spinn det opp via: (NB! Bruk rett filnavn ift. plassering osv. Bruk tabs for å fullføre filnavnet)
```
kubectl create -f deploy-basic.yaml
```

## 2.2 - Expose APIet utad
Her skal vi gjøre at domenet `api.local` kan brukes til å nå APIet vårt. 
Da må vi først lage en service som tar hånd om å velge pods. Deretter lager vi en ingress-regel for å knytte innkommende trafikk til servicen. Til slutt registrerer vi domenet lokalt. 
### 2.2.1 - Lag en Service
Så må vi få eksponert appen utad også
```
kubectl create -f service.yaml
```

### 2.2.2 - Sett opp ingress
Først - sett opp en ingress-tjeneste. Vi skal ikke i stor detalj her, så vi har en ferdig-kokt yaml:
```
kubectl create -f ingress.yaml
```
Denne knytter domenet 'api.local' til servicen vi lagde i forrige steg.

### 2.2.3 - Registrere domenet
Hos kunde bruker du en leverandør til å sette opp ekte domener. Her skal vi kun jobbe _lokalt_, så vi bruker heller en liten juksekode:
```
sudo sh -c "echo '$(minikube ip) api.local' >> /etc/hosts"
```

Denne legger til en linje i `/etc/hosts`, som gjør at maskina di bruker IPen til clusteret når du skriver `api.local` i nettleseren.

### 2.2.4 - Test
Gå til nettleseren din, og åpne `api.local/swagger` 
Voila! 

# Pause

# Del 3: Mer robusthet, Oppdateringer
## 2.1 Gjøre ting 
 - Sette på readiness- og livenessProbes
 - Oppdater til neste versjon. Denne krever tilkobling til DB og failer derfor
	 - Her kan man eventuelt velge å bruke en strategi som canary eller blue/green
 - Rull tilbake. 
 - Gå til neste seksjon -> lage en database
## 2.2 Slides om åssen deployments endres, replicasets osv 

# Pause 