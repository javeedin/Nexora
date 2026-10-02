# Liquibase + Oracle JDBC driver (Liquibase 5 ships only H2). Used by `make db-*` and CI.
FROM liquibase/liquibase:5.0.4@sha256:060f96efff4811ccf03957ff3160f21ea04139b89940640f8ad54a065cdc6906
ADD --chown=liquibase:liquibase --checksum=sha256:764cc3f88454d1be117e155d01ed617467b984c9c68f13090dd6083799d2bb8c \
    https://repo1.maven.org/maven2/com/oracle/database/jdbc/ojdbc11/23.26.3.0.0/ojdbc11-23.26.3.0.0.jar /liquibase/lib/
COPY --chown=liquibase:liquibase . /liquibase/changelog/
WORKDIR /liquibase/changelog
USER liquibase
ENTRYPOINT ["bash", "migrate.sh"]
